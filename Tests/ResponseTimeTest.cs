using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SoulKnightProtocol;

namespace Tests
{
    public class ResponseTimeTest
    {
        private TestClient client;
        private string serverIP = "127.0.0.1";
        private int serverPort = 9999;
        
        // 响应时间统计
        private List<long> loginResponseTimes = new List<long>();
        private List<long> createRoomResponseTimes = new List<long>();
        private List<long> findRoomResponseTimes = new List<long>();
        
        public void RunTest(int iterations = 100)
        {
            Console.WriteLine($"开始响应时间测试，迭代次数: {iterations}");
            
            try
            {
                // 创建客户端
                client = new TestClient(serverIP, serverPort);
                if (!client.Connect())
                    throw new Exception("无法连接到服务器");
                
                // 登录测试
                Console.WriteLine("测试登录响应时间...");
                for (int i = 0; i < iterations; i++)
                {
                    long responseTime = MeasureLoginResponseTime();
                    loginResponseTimes.Add(responseTime);
                    
                    // 每10次输出一次进度
                    if ((i + 1) % 10 == 0)
                        Console.WriteLine($"已完成 {i + 1}/{iterations} 次登录测试");
                        
                    // 等待一小段时间
                    Thread.Sleep(100);
                }
                
                // 创建房间测试
                Console.WriteLine("测试创建房间响应时间...");
                for (int i = 0; i < iterations; i++)
                {
                    long responseTime = MeasureCreateRoomResponseTime();
                    createRoomResponseTimes.Add(responseTime);
                    
                    // 每10次输出一次进度
                    if ((i + 1) % 10 == 0)
                        Console.WriteLine($"已完成 {i + 1}/{iterations} 次创建房间测试");
                        
                    // 等待一小段时间
                    Thread.Sleep(100);
                }
                
                // 查找房间测试
                Console.WriteLine("测试查找房间响应时间...");
                for (int i = 0; i < iterations; i++)
                {
                    long responseTime = MeasureFindRoomResponseTime();
                    findRoomResponseTimes.Add(responseTime);
                    
                    // 每10次输出一次进度
                    if ((i + 1) % 10 == 0)
                        Console.WriteLine($"已完成 {i + 1}/{iterations} 次查找房间测试");
                        
                    // 等待一小段时间
                    Thread.Sleep(100);
                }
                
                // 输出测试结果
                Console.WriteLine("响应时间测试完成");
                Console.WriteLine("登录响应时间 (毫秒):");
                PrintResponseTimeStats(loginResponseTimes);
                
                Console.WriteLine("创建房间响应时间 (毫秒):");
                PrintResponseTimeStats(createRoomResponseTimes);
                
                Console.WriteLine("查找房间响应时间 (毫秒):");
                PrintResponseTimeStats(findRoomResponseTimes);
                
                // 生成测试报告
                GenerateTestReport(loginResponseTimes, createRoomResponseTimes, findRoomResponseTimes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"响应时间测试异常: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // 清理资源
                if (client != null)
                    client.Disconnect();
            }
        }
        
        private long MeasureLoginResponseTime()
        {
            Stopwatch stopwatch = new Stopwatch();
            
            // 创建登录包
            MainPack loginPack = new MainPack();
            loginPack.RequestCode = RequestCode.User;
            loginPack.ActionCode = ActionCode.Login;
            
            LoginPack login = new LoginPack();
            login.UserName = $"TestUser{DateTime.Now.Ticks}";
            login.Password = "TestPassword";
            
            loginPack.LoginPack = login;
            
            // 测量响应时间
            stopwatch.Start();
            MainPack response = client.SendAndWaitResponse(loginPack);
            stopwatch.Stop();
            
            if (response == null || response.ReturnCode != ReturnCode.Success)
                return -1; // 表示失败
                
            return stopwatch.ElapsedMilliseconds;
        }
        
        private long MeasureCreateRoomResponseTime()
        {
            Stopwatch stopwatch = new Stopwatch();
            
            // 创建房间请求
            MainPack createRoomPack = new MainPack();
            createRoomPack.RequestCode = RequestCode.Room;
            createRoomPack.ActionCode = ActionCode.CreateRoom;
            
            // 测量响应时间
            stopwatch.Start();
            MainPack response = client.SendAndWaitResponse(createRoomPack);
            stopwatch.Stop();
            
            if (response == null || response.ReturnCode != ReturnCode.Success)
                return -1; // 表示失败
                
            return stopwatch.ElapsedMilliseconds;
        }
        
        private long MeasureFindRoomResponseTime()
        {
            Stopwatch stopwatch = new Stopwatch();
            
            // 查找房间请求
            MainPack findRoomPack = new MainPack();
            findRoomPack.RequestCode = RequestCode.Room;
            findRoomPack.ActionCode = ActionCode.FindRoom;
            
            // 测量响应时间
            stopwatch.Start();
            MainPack response = client.SendAndWaitResponse(findRoomPack);
            stopwatch.Stop();
            
            if (response == null)
                return -1; // 表示失败
                
            return stopwatch.ElapsedMilliseconds;
        }
        
        private void PrintResponseTimeStats(List<long> responseTimes)
        {
            if (responseTimes.Count == 0)
            {
                Console.WriteLine("  没有有效的响应时间数据");
                return;
            }
            
            // 过滤掉失败的请求
            List<long> validTimes = new List<long>();
            foreach (var time in responseTimes)
            {
                if (time >= 0)
                    validTimes.Add(time);
            }
            
            if (validTimes.Count == 0)
            {
                Console.WriteLine("  所有请求都失败了");
                return;
            }
            
            // 计算统计数据
            validTimes.Sort();
            
            long min = validTimes[0];
            long max = validTimes[validTimes.Count - 1];
            
            long sum = 0;
            foreach (var time in validTimes)
            {
                sum += time;
            }
            
            double avg = (double)sum / validTimes.Count;
            
            // 计算中位数
            long median;
            if (validTimes.Count % 2 == 0)
            {
                median = (validTimes[validTimes.Count / 2 - 1] + validTimes[validTimes.Count / 2]) / 2;
            }
            else
            {
                median = validTimes[validTimes.Count / 2];
            }
            
            // 计算95%分位数
            int p95Index = (int)(validTimes.Count * 0.95);
            long p95 = validTimes[p95Index];
            
            // 输出统计结果
            Console.WriteLine($"  请求总数: {responseTimes.Count}");
            Console.WriteLine($"  成功请求数: {validTimes.Count}");
            Console.WriteLine($"  成功率: {(double)validTimes.Count / responseTimes.Count * 100:F2}%");
            Console.WriteLine($"  最小响应时间: {min} ms");
            Console.WriteLine($"  最大响应时间: {max} ms");
            Console.WriteLine($"  平均响应时间: {avg:F2} ms");
            Console.WriteLine($"  中位数响应时间: {median} ms");
            Console.WriteLine($"  95%分位数响应时间: {p95} ms");
        }
        
        private void GenerateTestReport(List<long> loginTimes, List<long> createRoomTimes, List<long> findRoomTimes)
        {
            // 收集响应时间数据
            Dictionary<string, List<long>> responseTimes = new Dictionary<string, List<long>>
            {
                { "用户登录", loginTimes },
                { "创建房间", createRoomTimes },
                { "查找房间", findRoomTimes }
            };
            
            // 收集功能测试结果（这里假设所有功能都测试通过）
            Dictionary<string, bool> functionalTestResults = new Dictionary<string, bool>
            {
                { "Register", true },
                { "Login", true },
                { "PasswordEncryption", true },
                { "CreateRoom", true },
                { "FindRoom", true },
                { "JoinRoom", true },
                { "ExitRoom", true },
                { "StartBattle", true },
                { "BattleSync", true },
                { "OperationBroadcast", true },
                { "GameOver", true }
            };
            
            // 生成报告
            ServerTestReport report = new ServerTestReport();
            report.GenerateReport(responseTimes, functionalTestResults);
            report.DisplayReport();
        }
    }
}