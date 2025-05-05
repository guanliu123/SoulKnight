using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SoulKnightProtocol;

namespace Tests
{
    public class StressTest
    {
        private List<TestClient> clients = new List<TestClient>();
        private string serverIP = "127.0.0.1";
        private int serverPort = 9999;
        private int maxClients = 100; // 最大客户端数量
        private int activeClients = 0; // 当前活跃客户端数量
        private int successCount = 0;
        private int errorCount = 0;
        private object lockObj = new object();
        private bool isRunning = false;
        
        // 性能监控
        private Process serverProcess;
        private Stopwatch testStopwatch = new Stopwatch();
        private List<float> cpuUsageHistory = new List<float>();
        private List<long> memoryUsageHistory = new List<long>();
        
        public StressTest(int clientCount = 100)
        {
            maxClients = clientCount;
        }
        
        public void RunTest(int testDurationSeconds = 300)
        {
            Console.WriteLine($"开始压力测试，目标客户端数: {maxClients}，持续时间: {testDurationSeconds} 秒");
            
            try
            {
                // 查找服务器进程
                Process[] processes = Process.GetProcessesByName("Server");
                if (processes.Length > 0)
                {
                    serverProcess = processes[0];
                    Console.WriteLine($"找到服务器进程，PID: {serverProcess.Id}");
                }
                else
                {
                    Console.WriteLine("警告: 未找到服务器进程，无法监控CPU使用情况");
                }
                
                isRunning = true;
                testStopwatch.Start();
                
                // 启动监控线程
                Task monitorTask = Task.Run(() => MonitorPerformance());
                
                // 逐步增加客户端数量
                Task clientTask = Task.Run(() => GraduallyIncreaseClients());
                
                // 等待测试时间结束
                Thread.Sleep(testDurationSeconds * 1000);
                
                // 停止测试
                isRunning = false;
                testStopwatch.Stop();
                
                // 等待线程结束
                clientTask.Wait();
                monitorTask.Wait();
                
                // 输出测试结果
                Console.WriteLine("压力测试完成");
                Console.WriteLine($"最大客户端数: {activeClients}");
                Console.WriteLine($"总请求数: {successCount + errorCount}");
                Console.WriteLine($"成功请求数: {successCount}");
                Console.WriteLine($"失败请求数: {errorCount}");
                Console.WriteLine($"成功率: {(double)successCount / (successCount + errorCount) * 100:F2}%");
                Console.WriteLine($"每秒请求数: {(double)(successCount + errorCount) / testStopwatch.Elapsed.TotalSeconds:F2}");
                
                // 输出CPU和内存使用情况
                if (cpuUsageHistory.Count > 0)
                {
                    float avgCpu = 0;
                    foreach (var cpu in cpuUsageHistory)
                    {
                        avgCpu += cpu;
                    }
                    avgCpu /= cpuUsageHistory.Count;
                    
                    float maxCpu = 0;
                    foreach (var cpu in cpuUsageHistory)
                    {
                        if (cpu > maxCpu)
                            maxCpu = cpu;
                    }
                    
                    Console.WriteLine($"平均CPU使用率: {avgCpu:F2}%");
                    Console.WriteLine($"最大CPU使用率: {maxCpu:F2}%");
                }
                
                if (memoryUsageHistory.Count > 0)
                {
                    long avgMemory = 0;
                    foreach (var mem in memoryUsageHistory)
                    {
                        avgMemory += mem;
                    }
                    avgMemory /= memoryUsageHistory.Count;
                    
                    long maxMemory = 0;
                    foreach (var mem in memoryUsageHistory)
                    {
                        if (mem > maxMemory)
                            maxMemory = mem;
                    }
                    
                    Console.WriteLine($"平均内存使用: {avgMemory / 1024 / 1024} MB");
                    Console.WriteLine($"最大内存使用: {maxMemory / 1024 / 1024} MB");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"压力测试异常: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // 清理资源
                foreach (var client in clients)
                {
                    client.Disconnect();
                }
            }
        }
        
        private void GraduallyIncreaseClients()
        {
            int batchSize = 10; // 每批增加的客户端数量
            int batchInterval = 10000; // 每批之间的间隔时间(毫秒)
            
            while (isRunning && activeClients < maxClients)
            {
                int newClientsCount = Math.Min(batchSize, maxClients - activeClients);
                
                Console.WriteLine($"添加 {newClientsCount} 个新客户端，当前总数: {activeClients + newClientsCount}");
                
                // 创建新的客户端
                for (int i = 0; i < newClientsCount; i++)
                {
                    int clientIndex = activeClients + i;
                    
                    TestClient client = new TestClient(serverIP, serverPort);
                    if (client.Connect())
                    {
                        // 登录
                        MainPack loginPack = new MainPack();
                        loginPack.RequestCode = RequestCode.User;
                        loginPack.ActionCode = ActionCode.Login;
                        
                        LoginPack login = new LoginPack();
                        login.UserName = $"StressUser{clientIndex}";
                        login.Password = "password";
                        
                        loginPack.LoginPack = login;
                        
                        MainPack response = client.SendAndWaitResponse(loginPack);
                        
                        if (response != null && response.ReturnCode == ReturnCode.Success)
                        {
                            client.IsLoggedIn = true;
                            client.UserId = clientIndex + 1;
                            clients.Add(client);
                            
                            // 启动客户端测试线程
                            Task.Run(() => RunClientOperations(client));
                            
                            lock (lockObj)
                            {
                                successCount++;
                            }
                        }
                        else
                        {
                            client.Disconnect();
                            
                            lock (lockObj)
                            {
                                errorCount++;
                            }
                        }
                    }
                    else
                    {
                        lock (lockObj)
                        {
                            errorCount++;
                        }
                    }
                }
                
                activeClients += newClientsCount;
                
                // 等待一段时间，让服务器稳定
                Thread.Sleep(batchInterval);
                
                // 检查服务器是否仍然响应
                if (!CheckServerResponsive())
                {
                    Console.WriteLine("服务器无响应，停止增加客户端");
                    break;
                }
            }
        }
        
        private bool CheckServerResponsive()
        {
            if (clients.Count == 0)
                return false;
                
            // 随机选择一个客户端发送心跳
            Random random = new Random();
            int index = random.Next(0, clients.Count);
            
            TestClient client = clients[index];
            
            // 使用登录请求作为心跳检测
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.Login;
            
            LoginPack login = new LoginPack();
            login.UserName = "heartbeat";
            login.Password = "heartbeat";
            
            pack.LoginPack = login;
            
            MainPack response = client.SendAndWaitResponse(pack, 2000); // 2秒超时
            
            return response != null;
        }
        
        private void RunClientOperations(TestClient client)
        {
            Random random = new Random(client.UserId);
            
            while (isRunning && client.IsLoggedIn)
            {
                try
                {
                    // 随机选择一个操作
                    int operation = random.Next(0, 3);
                    
                    switch (operation)
                    {
                        case 0: // 创建房间
                            TestCreateRoom(client);
                            break;
                        case 1: // 获取房间列表
                            TestFindRoom(client);
                            break;
                        case 2: // 加入房间
                            TestJoinRoom(client);
                            break;
                    }
                    
                    // 随机等待一段时间
                    Thread.Sleep(random.Next(100, 500));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"客户端 {client.UserId} 操作异常: {ex.Message}");
                    
                    lock (lockObj)
                    {
                        errorCount++;
                    }
                }
            }
        }
        
        private void TestCreateRoom(TestClient client)
        {
            if (client.RoomId > 0)
                return;
                
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Room;
            pack.ActionCode = ActionCode.CreateRoom;
            
            MainPack response = client.SendAndWaitResponse(pack, 1000);
            
            if (response != null && response.ReturnCode == ReturnCode.Success)
            {
                client.RoomId = int.Parse(response.Str);
                
                lock (lockObj)
                {
                    successCount++;
                }
            }
            else
            {
                lock (lockObj)
                {
                    errorCount++;
                }
            }
        }
        
        private void TestFindRoom(TestClient client)
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Room;
            pack.ActionCode = ActionCode.FindRoom;
            
            MainPack response = client.SendAndWaitResponse(pack, 1000);
            
            if (response != null)
            {
                lock (lockObj)
                {
                    successCount++;
                }
            }
            else
            {
                lock (lockObj)
                {
                    errorCount++;
                }
            }
        }
        
        private void TestJoinRoom(TestClient client)
        {
            if (client.RoomId > 0)
                return;
                
            // 获取房间列表
            MainPack listPack = new MainPack();
            listPack.RequestCode = RequestCode.Room;
            listPack.ActionCode = ActionCode.FindRoom;
            
            MainPack listResponse = client.SendAndWaitResponse(listPack, 1000);
            
            if (listResponse != null && listResponse.RoomPacks.Count > 0)
            {
                // 随机选择一个房间加入
                Random random = new Random();
                int index = random.Next(0, listResponse.RoomPacks.Count);
                
                int roomId = listResponse.RoomPacks[index].RoomID;
                
                MainPack joinPack = new MainPack();
                joinPack.RequestCode = RequestCode.Room;
                joinPack.ActionCode = ActionCode.JoinRoom;
                joinPack.Str = roomId.ToString();
                
                MainPack joinResponse = client.SendAndWaitResponse(joinPack, 1000);
                
                if (joinResponse != null && joinResponse.ReturnCode == ReturnCode.Success)
                {
                    client.RoomId = roomId;
                    
                    lock (lockObj)
                    {
                        successCount++;
                    }
                }
                else
                {
                    lock (lockObj)
                    {
                        errorCount++;
                    }
                }
            }
        }
        
        private void MonitorPerformance()
        {
            if (serverProcess == null)
                return;
                
            PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", serverProcess.ProcessName);
            
            while (isRunning)
            {
                try
                {
                    // 刷新进程信息
                    serverProcess.Refresh();
                    
                    // 获取CPU使用率
                    float cpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
                    cpuUsageHistory.Add(cpuUsage);
                    
                    // 获取内存使用
                    long memoryUsage = serverProcess.WorkingSet64;
                    memoryUsageHistory.Add(memoryUsage);
                    
                    // 每秒记录一次
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"监控性能异常: {ex.Message}");
                    break;
                }
            }
        }
    }
}