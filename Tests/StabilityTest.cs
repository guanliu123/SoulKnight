using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SoulKnightProtocol;

namespace Tests
{
    public class StabilityTest
    {
        private List<TestClient> clients = new List<TestClient>();
        private string serverIP = "127.0.0.1";
        private int serverPort = 9999;
        private bool isRunning = false;
        private int errorCount = 0;
        private int successCount = 0;
        private object lockObj = new object();
        
        public void RunTest(int durationSeconds = 3600)
        {
            Console.WriteLine($"开始稳定性测试，持续时间: {durationSeconds} 秒");
            
            try
            {
                isRunning = true;
                
                // 创建并连接客户端
                for (int i = 0; i < 5; i++)
                {
                    TestClient client = new TestClient(serverIP, serverPort);
                    if (client.Connect())
                    {
                        // 登录
                        MainPack loginPack = new MainPack();
                        loginPack.RequestCode = RequestCode.User;
                        loginPack.ActionCode = ActionCode.Login;
                        
                        LoginPack login = new LoginPack();
                        login.UserName = $"StabilityUser{i}";  // 修改 Username 为 UserName
                        login.Password = "password";
                        
                        loginPack.LoginPack = login;
                        
                        MainPack response = client.SendAndWaitResponse(loginPack);
                        
                        if (response != null && response.ReturnCode == ReturnCode.Success)
                        {
                            client.IsLoggedIn = true;
                            client.UserId = i + 1;
                            clients.Add(client);
                            
                            // 启动客户端测试线程
                            Task.Run(() => RunClientOperations(client));
                        }
                        else
                        {
                            client.Disconnect();
                        }
                    }
                }
                
                // 启动监控线程
                Task monitorTask = Task.Run(() => MonitorClients());
                
                // 等待测试时间结束
                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalSeconds < durationSeconds && isRunning)
                {
                    Thread.Sleep(1000);
                    
                    // 每分钟输出一次状态
                    if ((DateTime.Now - startTime).TotalSeconds % 60 == 0)
                    {
                        Console.WriteLine($"稳定性测试运行中... 已运行: {(DateTime.Now - startTime).TotalMinutes:F1} 分钟");
                        Console.WriteLine($"成功请求: {successCount}, 失败请求: {errorCount}");
                    }
                }
                
                isRunning = false;
                
                // 等待监控线程结束
                monitorTask.Wait();
                
                // 输出测试结果
                Console.WriteLine("稳定性测试完成");
                Console.WriteLine($"总运行时间: {(DateTime.Now - startTime).TotalMinutes:F1} 分钟");
                Console.WriteLine($"成功请求数: {successCount}");
                Console.WriteLine($"失败请求数: {errorCount}");
                Console.WriteLine($"成功率: {(double)successCount / (successCount + errorCount) * 100:F2}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"稳定性测试异常: {ex.Message}");
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
        
        private void RunClientOperations(TestClient client)
        {
            Random random = new Random(client.UserId);
            
            while (isRunning && client.IsConnected)
            {
                try
                {
                    // 随机选择一个操作
                    int operation = random.Next(0, 4);
                    
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
                        case 3: // 心跳
                            TestHeartbeat(client);
                            break;
                    }
                    
                    // 随机等待一段时间
                    Thread.Sleep(random.Next(500, 2000));
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
            
            MainPack response = client.SendAndWaitResponse(pack);
            
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
            
            MainPack response = client.SendAndWaitResponse(pack);
            
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
            
            MainPack listResponse = client.SendAndWaitResponse(listPack);
            
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
                
                MainPack joinResponse = client.SendAndWaitResponse(joinPack);
                
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
        
        private void TestHeartbeat(TestClient client)
        {
            // 使用登录请求作为心跳检测
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.Login;
            
            LoginPack login = new LoginPack();
            login.UserName = "heartbeat";  // 修改 Username 为 UserName
            login.Password = "heartbeat";
            
            pack.LoginPack = login;
            
            MainPack response = client.SendAndWaitResponse(pack, 2000); // 2秒超时
            
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
        
        private void MonitorClients()
        {
            while (isRunning)
            {
                // 检查所有客户端的连接状态
                for (int i = 0; i < clients.Count; i++)
                {
                    if (!clients[i].IsConnected)
                    {
                        Console.WriteLine($"客户端 {clients[i].UserId} 断开连接，尝试重新连接...");
                        
                        // 尝试重新连接
                        if (clients[i].Connect())
                        {
                            Console.WriteLine($"客户端 {clients[i].UserId} 重新连接成功");
                            
                            // 重新登录
                            MainPack loginPack = new MainPack();
                            loginPack.RequestCode = RequestCode.User;
                            loginPack.ActionCode = ActionCode.Login;
                            
                            LoginPack login = new LoginPack();
                            login.UserName = $"StabilityUser{clients[i].UserId}";  // 修改 Username 为 UserName
                            login.Password = "password";
                            
                            loginPack.LoginPack = login;
                            
                            MainPack response = clients[i].SendAndWaitResponse(loginPack);
                            
                            if (response != null && response.ReturnCode == ReturnCode.Success)
                            {
                                clients[i].IsLoggedIn = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"客户端 {clients[i].UserId} 重新连接失败");
                        }
                    }
                }
                
                // 每5秒检查一次
                Thread.Sleep(5000);
            }
        }
    }
}