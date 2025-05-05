using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SoulKnightProtocol;

namespace TestJsonProto
{
    public class ProtobufVsJsonTest
    {
        private const int TEST_ITERATIONS = 1000;
        private readonly string TEST_RESULT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", "ProtobufVsJsonTest_Result.md");
        private readonly string CHART_DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", "ProtobufVsJsonTest_Data.csv");
        
        private readonly int[] dataSizes = { 1, 10, 50, 100, 500, 1000 };
        
        private Dictionary<string, List<double>> serializationTimes;
        private Dictionary<string, List<double>> deserializationTimes;
        private Dictionary<string, List<double>> compressionRatios;
        private Dictionary<string, List<double>> transmissionTimes;
        
        public void RunTest()
        {
            Console.WriteLine("开始 Protobuf 与 JSON 性能对比测试...");
            
            // 初始化结果集合
            serializationTimes = new Dictionary<string, List<double>>
            {
                { "Protobuf", new List<double>() },
                { "JSON", new List<double>() }
            };
            
            deserializationTimes = new Dictionary<string, List<double>>
            {
                { "Protobuf", new List<double>() },
                { "JSON", new List<double>() }
            };
            
            compressionRatios = new Dictionary<string, List<double>>
            {
                { "Protobuf", new List<double>() },
                { "JSON", new List<double>() }
            };
            
            transmissionTimes = new Dictionary<string, List<double>>
            {
                { "Protobuf", new List<double>() },
                { "JSON", new List<double>() }
            };
            
            // 确保结果目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(TEST_RESULT_PATH));
            
            // 对不同大小的数据进行测试
            foreach (int size in dataSizes)
            {
                Console.WriteLine($"\n测试数据大小: {size} 条目");
                
                // 生成测试数据
                MainPack protobufData = GenerateTestData(size);
                string jsonData = ConvertToJson(protobufData);
                
                // 测试序列化性能
                TestSerialization(protobufData, jsonData);
                
                // 测试反序列化性能
                TestDeserialization(protobufData, jsonData);
                
                // 测试压缩率
                TestCompressionRatio(protobufData, jsonData);
                
                // 测试网络传输性能
                TestNetworkTransmission(protobufData, jsonData, size);
            }
            
            // 生成测试报告
            GenerateTestReport();
            
            // 生成图表数据
            GenerateChartData();
            Console.WriteLine("\n测试完成。结果已保存到:");
            Console.WriteLine($"- 报告: {TEST_RESULT_PATH}");
            Console.WriteLine($"- 图表数据: {CHART_DATA_PATH}");
            Console.WriteLine($"- HTML 图表: {Path.Combine(Path.GetDirectoryName(TEST_RESULT_PATH), "ProtobufVsJsonTest_Chart.html")}");
        }
        
        private MainPack GenerateTestData(int size)
        {
            MainPack pack = new MainPack
            {
                ActionCode = ActionCode.BattleStart, // 修改为proto中存在的枚举值
                RequestCode = RequestCode.Room,
                ReturnCode = ReturnCode.Success,
                LoginPack = new LoginPack
                {
                    UserName = "TestUser",
                    Password = "TestPassword"
                }
            };
            
            // 添加房间数据
            for (int i = 0; i < Math.Min(size, 10); i++)
            {
                RoomPack roomPack = new RoomPack
                {
                    RoomName = $"Room_{i}",
                    CurrentNum = i % 4 + 1, // 修改为CurrentNum
                    MaxNum = 4, // 修改为MaxNum
                    RoomCode = i % 2 == 0 ? RoomCode.WaitForJoin : RoomCode.Playing // 修改为RoomCode
                };
                
                // 添加玩家数据
                for (int j = 0; j < roomPack.CurrentNum; j++)
                {
                    roomPack.PlayerPacks.Add(new PlayerPack
                    {
                        PlayerID = j + 1,
                        PlayerName = $"Player_{j}",
                    });
                }
                
                pack.RoomPacks.Add(roomPack);
            }
            
            // 添加角色数据
            for (int i = 0; i < size; i++)
            {
                CharacterPack characterPack = new CharacterPack
                {
                    CharacterName = $"Character_{i}",
                    PlayerType = $"Type_{i % 5}",
                    InputPack = new InputPack // 修正 InputPack 字段
                    {
                        Horizontal = (float)Math.Sin(i),
                        Vertical = (float)Math.Cos(i),
                        MousePosX = i * 10,
                        MousePosY = i * 5,
                        CharacterPosX = i * 2, // 添加 proto 中定义的字段
                        CharacterPosY = i * 3, // 添加 proto 中定义的字段
                        IsAttackKeyDown = i % 3 == 0, // 使用 proto 中定义的字段
                        IsSkillKeyDown = i % 5 == 0,  // 使用 proto 中定义的字段
                        IsSwitchKeyDown = i % 7 == 0, // 使用 proto 中定义的字段
                        IsInteractKeyDown = i % 9 == 0,// 使用 proto 中定义的字段
                        BattleId = i % 4,             // 添加 proto 中定义的字段
                        FrameId = i                   // 添加 proto 中定义的字段
                    }
                };

                pack.CharacterPacks.Add(characterPack);
            }

            // 添加战斗信息
            pack.BattleInfo = new BattleInfo
            {
                OperationID = 12345
            };

            // 添加战斗用户信息 (使用 BattlePlayerPack)
            for (int i = 0; i < Math.Min(size, 4); i++)
            {
                // proto 中 BattleInfo.BattleUserInfo 是 repeated BattlePlayerPack
                pack.BattleInfo.BattleUserInfo.Add(new BattlePlayerPack
                {
                    Id = i + 1, // 使用 BattlePlayerPack 的字段
                    Battleid = i, // 使用 BattlePlayerPack 的字段
                    Playername = $"BattlePlayer_{i}", // 使用 BattlePlayerPack 的字段
                    Hero = i % 3, // 使用 BattlePlayerPack 的字段
                    Teamid = i % 2, // 使用 BattlePlayerPack 的字段
                    SocketIP = $"192.168.1.{i + 1}" // 使用 BattlePlayerPack 的字段
                    // 移除 IsHouseOwner 和 IsReady，因为它们不在 BattlePlayerPack 中
                });
            }

            // 添加所有玩家操作
            for (int i = 0; i < size; i++)
            {
                // proto 中是 Frameid (小写 i)
                AllPlayerOperation operation = new AllPlayerOperation
                {
                   Frameid = i
                };

                // 添加操作
                for (int j = 0; j < Math.Min(size, 4); j++)
                {
                    operation.Operations.Add(new InputPack // 修正 InputPack 字段
                    {
                        Horizontal = (float)Math.Sin(i + j),
                        Vertical = (float)Math.Cos(i + j),
                        MousePosX = (i + j) * 10,
                        MousePosY = (i + j) * 5,
                        CharacterPosX = (i + j) * 2,
                        CharacterPosY = (i + j) * 3,
                        IsAttackKeyDown = (i + j) % 3 == 0,
                        IsSkillKeyDown = (i + j) % 5 == 0,
                        IsSwitchKeyDown = (i + j) % 7 == 0,
                        IsInteractKeyDown = (i + j) % 9 == 0,
                        BattleId = (i + j) % 4,
                        FrameId = i + j
                    });
                }

                pack.BattleInfo.AllPlayerOperation.Add(operation);
            }

            // 添加自己的操作
            pack.BattleInfo.SelfOperation = new InputPack // 修正 InputPack 字段
            {
                Horizontal = 0.5f,
                Vertical = 0.7f,
                MousePosX = 100,
                MousePosY = 200,
                CharacterPosX = 50,
                CharacterPosY = 60,
                IsAttackKeyDown = true,
                IsSkillKeyDown = true,
                IsSwitchKeyDown = false,
                IsInteractKeyDown = true,
                BattleId = 1,
                FrameId = 0 // 假设自己的操作帧ID为0
            };

            // 添加其他字段
            pack.Str = $"Test string with size {size}";

            // 可以选择性地填充 MainPack 中的 BattlePlayerPack 和 BattleInitInfo
            // pack.BattlePlayerPack.Add(...)
            // pack.BattleInitInfo = new BattleInitInfo { ... }

            return pack;
        }
        
        private string ConvertToJson(MainPack pack)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // 替换过时的IgnoreNullValues
            };
            
            return JsonSerializer.Serialize(pack, options);
        }
        
        private MainPack ConvertFromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // 替换过时的IgnoreNullValues
            };
            
            return JsonSerializer.Deserialize<MainPack>(json, options);
        }
        
        private void TestSerialization(MainPack protobufData, string jsonData)
        {
            Console.WriteLine("测试序列化性能...");
            
            // 测试Protobuf序列化
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                byte[] bytes = protobufData.ToByteArray();
            }
            
            watch.Stop();
            double protobufTime = watch.ElapsedMilliseconds / (double)TEST_ITERATIONS;
            serializationTimes["Protobuf"].Add(protobufTime);
            
            // 测试JSON序列化
            watch.Reset();
            watch.Start();
            
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                string json = ConvertToJson(protobufData);
            }
            
            watch.Stop();
            double jsonTime = watch.ElapsedMilliseconds / (double)TEST_ITERATIONS;
            serializationTimes["JSON"].Add(jsonTime);
            
            Console.WriteLine($"Protobuf序列化: {protobufTime:F3}ms, JSON序列化: {jsonTime:F3}ms");
        }
        
        private void TestDeserialization(MainPack protobufData, string jsonData)
        {
            Console.WriteLine("测试反序列化性能...");
            
            // 准备序列化数据
            byte[] protobufBytes = protobufData.ToByteArray();
            
            // 测试Protobuf反序列化
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                MainPack pack = MainPack.Parser.ParseFrom(protobufBytes);
            }
            
            watch.Stop();
            double protobufTime = watch.ElapsedMilliseconds / (double)TEST_ITERATIONS;
            deserializationTimes["Protobuf"].Add(protobufTime);
            
            // 测试JSON反序列化
            watch.Reset();
            watch.Start();
            
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                MainPack pack = ConvertFromJson(jsonData);
            }
            
            watch.Stop();
            double jsonTime = watch.ElapsedMilliseconds / (double)TEST_ITERATIONS;
            deserializationTimes["JSON"].Add(jsonTime);
            
            Console.WriteLine($"Protobuf反序列化: {protobufTime:F3}ms, JSON反序列化: {jsonTime:F3}ms");
        }
        
        private void TestCompressionRatio(MainPack protobufData, string jsonData)
        {
            Console.WriteLine("测试压缩率...");
            
            // 准备序列化数据
            byte[] protobufBytes = protobufData.ToByteArray();
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            
            // 计算压缩率（相对于原始对象大小的估计）
            int estimatedObjectSize = EstimateObjectSize(protobufData);
            double protobufRatio = (double)protobufBytes.Length / estimatedObjectSize;
            double jsonRatio = (double)jsonBytes.Length / estimatedObjectSize;
            
            compressionRatios["Protobuf"].Add(protobufRatio);
            compressionRatios["JSON"].Add(jsonRatio);
            
            Console.WriteLine($"Protobuf大小: {protobufBytes.Length}字节, JSON大小: {jsonBytes.Length}字节");
            Console.WriteLine($"Protobuf压缩率: {protobufRatio:F3}, JSON压缩率: {jsonRatio:F3}");
            Console.WriteLine($"Protobuf比JSON小: {(double)jsonBytes.Length / protobufBytes.Length:F2}倍");
        }
        
        private int EstimateObjectSize(MainPack pack)
        {
            // 这只是一个粗略的估计，实际上很难准确计算对象在内存中的大小
            int size = 0;

            // 基本字段
            size += 20; // 枚举和布尔值

            // LoginPack
            if (pack.LoginPack != null)
            {
                size += (pack.LoginPack.UserName?.Length ?? 0) * 2;
                size += (pack.LoginPack.Password?.Length ?? 0) * 2;
            }
            
            // RoomPacks
            foreach (var room in pack.RoomPacks)
            {
                size += 20; // 基本字段
                size += (room.RoomName?.Length ?? 0) * 2;
                
                foreach (var player in room.PlayerPacks)
                {
                    size += 8; // PlayerID
                    size += (player.PlayerName?.Length ?? 0) * 2;
                }
            }
            
            // CharacterPacks
            foreach (var character in pack.CharacterPacks)
            {
                size += (character.CharacterName?.Length ?? 0) * 2;
                size += (character.PlayerType?.Length ?? 0) * 2;

                if (character.InputPack != null)
                {
                    // 估计 InputPack 大小 (包含所有字段)
                    size += 4 * 6; // 6个 float
                    size += 1 * 4; // 4个 bool
                    size += 4 * 2; // 2个 int32
                }
            }

            // 战斗信息
            if (pack.BattleInfo != null)
            {
                size += 4; // OperationID

                // 迭代 BattlePlayerPack
                foreach (var player in pack.BattleInfo.BattleUserInfo)
                {
                    size += 4 * 4; // Id, Battleid, Hero, Teamid (int32)
                    size += (player.Playername?.Length ?? 0) * 2;
                    size += (player.SocketIP?.Length ?? 0) * 2;
                }

                foreach (var operation in pack.BattleInfo.AllPlayerOperation)
                {
                    size += 4; // Frameid (int32)

                    foreach (var input in operation.Operations)
                    {
                        // 估计 InputPack 大小
                        size += 4 * 6; // 6个 float
                        size += 1 * 4; // 4个 bool
                        size += 4 * 2; // 2个 int32
                    }
                }

                if (pack.BattleInfo.SelfOperation != null)
                {
                    // 估计 InputPack 大小
                    size += 4 * 6; // 6个 float
                    size += 1 * 4; // 4个 bool
                    size += 4 * 2; // 2个 int32
                }
            }

            // 其他字段
            size += (pack.Str?.Length ?? 0) * 2;

            // 考虑 MainPack 中的 BattlePlayerPack 和 BattleInitInfo (如果填充了)
            // ...

            return Math.Max(size, 100); // 确保至少有一个基本大小
        }
        
        private void TestNetworkTransmission(MainPack protobufData, string jsonData, int size)
        {
            Console.WriteLine("测试网络传输性能...");
            
            // 准备序列化数据
            byte[] protobufBytes = protobufData.ToByteArray();
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            
            // 模拟网络传输（使用本地回环地址）
            double protobufTime = MeasureTransmissionTime(protobufBytes, 10);
            double jsonTime = MeasureTransmissionTime(jsonBytes, 10);
            
            transmissionTimes["Protobuf"].Add(protobufTime);
            transmissionTimes["JSON"].Add(jsonTime);
            
            Console.WriteLine($"Protobuf传输: {protobufTime:F3}ms, JSON传输: {jsonTime:F3}ms");
        }
        
                private double MeasureTransmissionTime(byte[] data, int iterations)
        {
            // 创建一个简单的TCP服务器和客户端来测量传输时间
            TcpListener server = null;
            TcpClient client = null;
            Task serverTask = null; // 将 serverTask 移到 try 外部以便 finally 访问

            try
            {
                // 启动服务器
                server = new TcpListener(IPAddress.Loopback, 0); // 监听本地回环地址的随机可用端口
                server.Start();
                int port = ((IPEndPoint)server.LocalEndpoint).Port;

                // 创建一个任务来异步接收数据
                serverTask = Task.Run(() =>
                {
                    using (TcpClient serverClient = server.AcceptTcpClient()) // 等待并接受一个客户端连接
                    using (NetworkStream stream = serverClient.GetStream()) // 获取网络流
                    {
                        byte[] lengthBuffer = new byte[4]; // 用于读取长度前缀
                        byte[] dataBuffer = new byte[65536]; // 初始数据缓冲区大小

                        for (int i = 0; i < iterations; i++) // 循环接收 'iterations' 次数据
                        {
                            // 1. 读取4字节的数据长度前缀
                            int totalBytesRead = 0;
                            while (totalBytesRead < 4)
                            {
                                int bytesRead = stream.Read(lengthBuffer, totalBytesRead, 4 - totalBytesRead);
                                if (bytesRead == 0) // 如果读取到0字节，表示连接已关闭
                                    throw new IOException($"Server: Connection closed prematurely while reading length (iteration {i}).");
                                totalBytesRead += bytesRead;
                            }
                            int dataLength = BitConverter.ToInt32(lengthBuffer, 0); // 将字节转换为整数长度

                            // 确保缓冲区足够大 (虽然对于测试数据64k通常足够)
                            if (dataLength > dataBuffer.Length)
                                dataBuffer = new byte[dataLength];

                            // 2. 根据读取到的长度，读取实际数据
                            totalBytesRead = 0;
                            while (totalBytesRead < dataLength)
                            {
                                int bytesRead = stream.Read(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                                if (bytesRead == 0) // 如果读取到0字节，表示连接已关闭
                                    throw new IOException($"Server: Connection closed prematurely while reading data (iteration {i}). Expected {dataLength}, got {totalBytesRead}.");
                                totalBytesRead += bytesRead;
                            }
                            // 成功读取一次完整的 长度+数据 包
                        }
                    }
                    // using 语句结束，服务器端连接在此处自动关闭
                });

                // 连接到服务器
                client = new TcpClient();
                client.Connect(IPAddress.Loopback, port); // 连接到服务器启动的端口

                using (NetworkStream stream = client.GetStream()) // 获取客户端的网络流
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start(); // 开始计时

                    for (int i = 0; i < iterations; i++) // 循环发送 'iterations' 次数据
                    {
                        // 1. 发送4字节的数据长度前缀
                        byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                        stream.Write(lengthBytes, 0, lengthBytes.Length);

                        // 2. 发送实际数据
                        stream.Write(data, 0, data.Length);
                        // stream.Flush(); // 通常不需要手动 Flush，TCP 会自行处理缓冲和发送
                    }

                    watch.Stop(); // 停止计时

                    // 等待服务器任务完成，确保服务器已接收完所有数据
                    try
                    {
                        serverTask.Wait(); // 等待服务器端 Task 执行完毕
                    }
                    catch (AggregateException ae)
                    {
                        // 如果服务器任务出现异常，在这里处理
                        ae.Handle(ex => {
                            Console.WriteLine($"Error in server task: {ex.Message}");
                            // 可以根据需要决定是否让测试失败或返回错误值
                            return true; // 标记为已处理
                        });
                        return -1; // 返回错误指示值
                    }

                    return watch.ElapsedMilliseconds / (double)iterations; // 返回平均传输时间
                }
            }
            catch (Exception ex) // 捕获客户端或其他地方可能发生的异常
            {
                 Console.WriteLine($"MeasureTransmissionTime Error: {ex.Message}");
                 return -1; // 返回错误指示值
            }
            finally // 确保资源被释放
            {
                client?.Close(); // 关闭客户端连接
                server?.Stop(); // 停止服务器监听

                // 确保服务器任务即使出错也能被观察到（避免未观察到的任务异常）
                if (serverTask != null && serverTask.IsFaulted)
                {
                    try { serverTask.Wait(); } catch (AggregateException) { /* 异常已在上面处理或记录 */ }
                }
            }
        }
// ... a lot of existing code ...
        
        private void GenerateTestReport()
        {
            using (StreamWriter writer = new StreamWriter(TEST_RESULT_PATH))
            {
                writer.WriteLine("# Protobuf 与 JSON 性能对比测试报告");
                writer.WriteLine($"测试时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"测试迭代次数: {TEST_ITERATIONS}");
                writer.WriteLine();
                
                writer.WriteLine("## 1. 序列化性能测试");
                writer.WriteLine("| 数据大小(条目数) | Protobuf序列化时间(ms) | JSON序列化时间(ms) | Protobuf/JSON比率 |");
                writer.WriteLine("|-----------------|----------------------|-------------------|-----------------|");
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    double protobufTime = serializationTimes["Protobuf"][i];
                    double jsonTime = serializationTimes["JSON"][i];
                    double ratio = protobufTime / jsonTime;
                    
                    writer.WriteLine($"| {dataSizes[i]} | {protobufTime:F3} | {jsonTime:F3} | {ratio:F3} |");
                }
                
                writer.WriteLine();
                writer.WriteLine("## 2. 反序列化性能测试");
                writer.WriteLine("| 数据大小(条目数) | Protobuf反序列化时间(ms) | JSON反序列化时间(ms) | Protobuf/JSON比率 |");
                writer.WriteLine("|-----------------|------------------------|--------------------|-----------------|");
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    double protobufTime = deserializationTimes["Protobuf"][i];
                    double jsonTime = deserializationTimes["JSON"][i];
                    double ratio = protobufTime / jsonTime;
                    
                    writer.WriteLine($"| {dataSizes[i]} | {protobufTime:F3} | {jsonTime:F3} | {ratio:F3} |");
                }
                
                writer.WriteLine();
                writer.WriteLine("## 3. 数据压缩率测试");
                writer.WriteLine("| 数据大小(条目数) | Protobuf大小(字节) | JSON大小(字节) | Protobuf/JSON大小比 |");
                writer.WriteLine("|-----------------|-------------------|---------------|-------------------|");
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    // 生成测试数据
                    MainPack protobufData = GenerateTestData(dataSizes[i]);
                    string jsonData = ConvertToJson(protobufData);
                    
                    byte[] protobufBytes = protobufData.ToByteArray();
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                    
                    double sizeRatio = (double)protobufBytes.Length / jsonBytes.Length;
                    
                    writer.WriteLine($"| {dataSizes[i]} | {protobufBytes.Length} | {jsonBytes.Length} | {sizeRatio:F3} |");
                }
                
                writer.WriteLine();
                writer.WriteLine("## 4. 网络传输性能测试");
                writer.WriteLine("| 数据大小(条目数) | Protobuf传输时间(ms) | JSON传输时间(ms) | Protobuf/JSON比率 |");
                writer.WriteLine("|-----------------|---------------------|-----------------|-----------------|");
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    double protobufTime = transmissionTimes["Protobuf"][i];
                    double jsonTime = transmissionTimes["JSON"][i];
                    double ratio = protobufTime / jsonTime;
                    
                    writer.WriteLine($"| {dataSizes[i]} | {protobufTime:F3} | {jsonTime:F3} | {ratio:F3} |");
                }
                
                writer.WriteLine();
                writer.WriteLine("## 5. 测试结论");
                writer.WriteLine();
                writer.WriteLine("### 5.1 序列化性能");
                writer.WriteLine($"- Protobuf序列化性能平均比JSON快 {CalculateAverageRatio(serializationTimes["JSON"], serializationTimes["Protobuf"]):F2} 倍");
                writer.WriteLine($"- 随着数据量增加，Protobuf的性能优势{(IsRatioIncreasing(serializationTimes["JSON"], serializationTimes["Protobuf"]) ? "更加明显" : "保持稳定")}");
                
                writer.WriteLine();
                writer.WriteLine("### 5.2 反序列化性能");
                writer.WriteLine($"- Protobuf反序列化性能平均比JSON快 {CalculateAverageRatio(deserializationTimes["JSON"], deserializationTimes["Protobuf"]):F2} 倍");
                writer.WriteLine($"- 随着数据量增加，Protobuf的性能优势{(IsRatioIncreasing(deserializationTimes["JSON"], deserializationTimes["Protobuf"]) ? "更加明显" : "保持稳定")}");
                
                writer.WriteLine();
                writer.WriteLine("### 5.3 数据压缩率");
                
                // 计算平均压缩率
                double avgProtobufSize = 0;
                double avgJsonSize = 0;
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    MainPack protobufData = GenerateTestData(dataSizes[i]);
                    string jsonData = ConvertToJson(protobufData);
                    
                    byte[] protobufBytes = protobufData.ToByteArray();
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                    
                    avgProtobufSize += protobufBytes.Length;
                    avgJsonSize += jsonBytes.Length;
                }
                
                avgProtobufSize /= dataSizes.Length;
                avgJsonSize /= dataSizes.Length;
                
                writer.WriteLine($"- Protobuf数据大小平均比JSON小 {avgJsonSize / avgProtobufSize:F2} 倍");
                writer.WriteLine("- Protobuf采用二进制格式，而JSON是文本格式，因此在数据压缩方面Protobuf具有明显优势");
                
                writer.WriteLine();
                writer.WriteLine("### 5.4 网络传输性能");
                writer.WriteLine($"- Protobuf网络传输性能平均比JSON快 {CalculateAverageRatio(transmissionTimes["JSON"], transmissionTimes["Protobuf"]):F2} 倍");
                writer.WriteLine($"- 随着数据量增加，Protobuf的传输性能优势{(IsRatioIncreasing(transmissionTimes["JSON"], transmissionTimes["Protobuf"]) ? "更加明显" : "保持稳定")}");
                
                writer.WriteLine();
                writer.WriteLine("### 5.5 总结");
                writer.WriteLine("- Protobuf在序列化、反序列化、数据压缩和网络传输方面都优于JSON");
                writer.WriteLine("- 对于需要高性能网络通信的应用，特别是数据量较大时，Protobuf是更好的选择");
                writer.WriteLine("- JSON的优势在于可读性和跨平台兼容性，适合需要人工查看或编辑的场景");
            }
            
            Console.WriteLine($"测试报告已生成: {TEST_RESULT_PATH}");
        }

        private void GenerateChartData()
        {
            Console.WriteLine("生成图表数据...");
            
            using (StreamWriter writer = new StreamWriter(CHART_DATA_PATH))
            {
                // 写入CSV头部
                writer.WriteLine("数据大小,Protobuf序列化时间,JSON序列化时间,Protobuf反序列化时间,JSON反序列化时间,Protobuf大小,JSON大小,Protobuf传输时间,JSON传输时间");
                
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    // 生成测试数据以获取大小信息
                    MainPack protobufData = GenerateTestData(dataSizes[i]);
                    string jsonData = ConvertToJson(protobufData);
                    
                    byte[] protobufBytes = protobufData.ToByteArray();
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                    
                    // 写入CSV行
                    writer.WriteLine($"{dataSizes[i]},{serializationTimes["Protobuf"][i]:F3},{serializationTimes["JSON"][i]:F3}," +
                                    $"{deserializationTimes["Protobuf"][i]:F3},{deserializationTimes["JSON"][i]:F3}," +
                                    $"{protobufBytes.Length},{jsonBytes.Length}," +
                                    $"{transmissionTimes["Protobuf"][i]:F3},{transmissionTimes["JSON"][i]:F3}");
                }
            }
            
            // 生成HTML图表文件
            GenerateHtmlChart();
        }
        
        private void GenerateHtmlChart()
        {
            string htmlPath = Path.Combine(Path.GetDirectoryName(TEST_RESULT_PATH), "ProtobufVsJsonTest_Chart.html");
            
            using (StreamWriter writer = new StreamWriter(htmlPath))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("    <title>Protobuf vs JSON 性能对比</title>");
                writer.WriteLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
                writer.WriteLine("    <style>");
                writer.WriteLine("        .chart-container {");
                writer.WriteLine("            width: 800px;");
                writer.WriteLine("            height: 400px;");
                writer.WriteLine("            margin: 20px auto;");
                writer.WriteLine("        }");
                writer.WriteLine("        h1, h2 {");
                writer.WriteLine("            text-align: center;");
                writer.WriteLine("        }");
                writer.WriteLine("    </style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                writer.WriteLine("    <h1>Protobuf vs JSON 性能对比</h1>");
                
                // 序列化性能图表
                writer.WriteLine("    <div class=\"chart-container\">");
                writer.WriteLine("        <h2>序列化性能</h2>");
                writer.WriteLine("        <canvas id=\"serializationChart\"></canvas>");
                writer.WriteLine("    </div>");
                
                // 反序列化性能图表
                writer.WriteLine("    <div class=\"chart-container\">");
                writer.WriteLine("        <h2>反序列化性能</h2>");
                writer.WriteLine("        <canvas id=\"deserializationChart\"></canvas>");
                writer.WriteLine("    </div>");
                
                // 数据大小图表
                writer.WriteLine("    <div class=\"chart-container\">");
                writer.WriteLine("        <h2>数据大小</h2>");
                writer.WriteLine("        <canvas id=\"sizeChart\"></canvas>");
                writer.WriteLine("    </div>");
                
                // 网络传输性能图表
                writer.WriteLine("    <div class=\"chart-container\">");
                writer.WriteLine("        <h2>网络传输性能</h2>");
                writer.WriteLine("        <canvas id=\"transmissionChart\"></canvas>");
                writer.WriteLine("    </div>");
                
                // JavaScript代码
                writer.WriteLine("    <script>");
                writer.WriteLine("        // 数据");
                                // JavaScript代码
                writer.WriteLine("    <script>");
                writer.WriteLine("        // 数据");
                writer.WriteLine($"        const dataSizes = [{string.Join(", ", dataSizes)}];");
                writer.WriteLine("        ");
                
                // 序列化时间数据
                writer.WriteLine("        const serializationData = {");
                writer.WriteLine($"            protobuf: [{string.Join(", ", serializationTimes["Protobuf"].Select(t => t.ToString("F3")))}],");
                writer.WriteLine($"            json: [{string.Join(", ", serializationTimes["JSON"].Select(t => t.ToString("F3")))}]");
                writer.WriteLine("        };");
                writer.WriteLine("        ");
                
                // 反序列化时间数据
                writer.WriteLine("        const deserializationData = {");
                writer.WriteLine($"            protobuf: [{string.Join(", ", deserializationTimes["Protobuf"].Select(t => t.ToString("F3")))}],");
                writer.WriteLine($"            json: [{string.Join(", ", deserializationTimes["JSON"].Select(t => t.ToString("F3")))}]");
                writer.WriteLine("        };");
                writer.WriteLine("        ");
                
                // 大小数据
                writer.WriteLine("        const sizeData = {");
                writer.WriteLine("            protobuf: [");
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    MainPack protobufData = GenerateTestData(dataSizes[i]);
                    byte[] protobufBytes = protobufData.ToByteArray();
                    writer.WriteLine($"                {protobufBytes.Length}" + (i < dataSizes.Length - 1 ? "," : ""));
                }
                writer.WriteLine("            ],");
                writer.WriteLine("            json: [");
                for (int i = 0; i < dataSizes.Length; i++)
                {
                    MainPack protobufData = GenerateTestData(dataSizes[i]);
                    string jsonData = ConvertToJson(protobufData);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                    writer.WriteLine($"                {jsonBytes.Length}" + (i < dataSizes.Length - 1 ? "," : ""));
                }
                writer.WriteLine("            ]");
                writer.WriteLine("        };");
                writer.WriteLine("        ");
                
                // 传输时间数据
                writer.WriteLine("        const transmissionData = {");
                writer.WriteLine($"            protobuf: [{string.Join(", ", transmissionTimes["Protobuf"].Select(t => t.ToString("F3")))}],");
                writer.WriteLine($"            json: [{string.Join(", ", transmissionTimes["JSON"].Select(t => t.ToString("F3")))}]");
                writer.WriteLine("        };");
                writer.WriteLine("        ");
                
                // 创建图表
                writer.WriteLine("        // 序列化性能图表");
                writer.WriteLine("        const serializationCtx = document.getElementById('serializationChart').getContext('2d');");
                writer.WriteLine("        new Chart(serializationCtx, {");
                writer.WriteLine("            type: 'line',");
                writer.WriteLine("            data: {");
                writer.WriteLine("                labels: dataSizes,");
                writer.WriteLine("                datasets: [");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'Protobuf',");
                writer.WriteLine("                        data: serializationData.protobuf,");
                writer.WriteLine("                        borderColor: 'rgba(75, 192, 192, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(75, 192, 192, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    },");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'JSON',");
                writer.WriteLine("                        data: serializationData.json,");
                writer.WriteLine("                        borderColor: 'rgba(255, 99, 132, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(255, 99, 132, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    }");
                writer.WriteLine("                ]");
                writer.WriteLine("            },");
                writer.WriteLine("            options: {");
                writer.WriteLine("                responsive: true,");
                writer.WriteLine("                scales: {");
                writer.WriteLine("                    x: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '数据大小 (条目数)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    },");
                writer.WriteLine("                    y: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '序列化时间 (ms)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    }");
                writer.WriteLine("                }");
                writer.WriteLine("            }");
                writer.WriteLine("        });");
                writer.WriteLine("        ");
                
                // 反序列化性能图表
                writer.WriteLine("        // 反序列化性能图表");
                writer.WriteLine("        const deserializationCtx = document.getElementById('deserializationChart').getContext('2d');");
                writer.WriteLine("        new Chart(deserializationCtx, {");
                writer.WriteLine("            type: 'line',");
                writer.WriteLine("            data: {");
                writer.WriteLine("                labels: dataSizes,");
                writer.WriteLine("                datasets: [");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'Protobuf',");
                writer.WriteLine("                        data: deserializationData.protobuf,");
                writer.WriteLine("                        borderColor: 'rgba(75, 192, 192, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(75, 192, 192, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    },");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'JSON',");
                writer.WriteLine("                        data: deserializationData.json,");
                writer.WriteLine("                        borderColor: 'rgba(255, 99, 132, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(255, 99, 132, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    }");
                writer.WriteLine("                ]");
                writer.WriteLine("            },");
                writer.WriteLine("            options: {");
                writer.WriteLine("                responsive: true,");
                writer.WriteLine("                scales: {");
                writer.WriteLine("                    x: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '数据大小 (条目数)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    },");
                writer.WriteLine("                    y: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '反序列化时间 (ms)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    }");
                writer.WriteLine("                }");
                writer.WriteLine("            }");
                writer.WriteLine("        });");
                writer.WriteLine("        ");
                
                // 数据大小图表
                writer.WriteLine("        // 数据大小图表");
                writer.WriteLine("        const sizeCtx = document.getElementById('sizeChart').getContext('2d');");
                writer.WriteLine("        new Chart(sizeCtx, {");
                writer.WriteLine("            type: 'bar',");
                writer.WriteLine("            data: {");
                writer.WriteLine("                labels: dataSizes,");
                writer.WriteLine("                datasets: [");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'Protobuf',");
                writer.WriteLine("                        data: sizeData.protobuf,");
                writer.WriteLine("                        backgroundColor: 'rgba(75, 192, 192, 0.6)'");
                writer.WriteLine("                    },");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'JSON',");
                writer.WriteLine("                        data: sizeData.json,");
                writer.WriteLine("                        backgroundColor: 'rgba(255, 99, 132, 0.6)'");
                writer.WriteLine("                    }");
                writer.WriteLine("                ]");
                writer.WriteLine("            },");
                writer.WriteLine("            options: {");
                writer.WriteLine("                responsive: true,");
                writer.WriteLine("                scales: {");
                writer.WriteLine("                    x: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '数据大小 (条目数)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    },");
                writer.WriteLine("                    y: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '字节数'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    }");
                writer.WriteLine("                }");
                writer.WriteLine("            }");
                writer.WriteLine("        });");
                writer.WriteLine("        ");
                
                // 网络传输性能图表
                writer.WriteLine("        // 网络传输性能图表");
                writer.WriteLine("        const transmissionCtx = document.getElementById('transmissionChart').getContext('2d');");
                writer.WriteLine("        new Chart(transmissionCtx, {");
                writer.WriteLine("            type: 'line',");
                writer.WriteLine("            data: {");
                writer.WriteLine("                labels: dataSizes,");
                writer.WriteLine("                datasets: [");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'Protobuf',");
                writer.WriteLine("                        data: transmissionData.protobuf,");
                writer.WriteLine("                        borderColor: 'rgba(75, 192, 192, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(75, 192, 192, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    },");
                writer.WriteLine("                    {");
                writer.WriteLine("                        label: 'JSON',");
                writer.WriteLine("                        data: transmissionData.json,");
                writer.WriteLine("                        borderColor: 'rgba(255, 99, 132, 1)',");
                writer.WriteLine("                        backgroundColor: 'rgba(255, 99, 132, 0.2)',");
                writer.WriteLine("                        tension: 0.1");
                writer.WriteLine("                    }");
                writer.WriteLine("                ]");
                writer.WriteLine("            },");
                writer.WriteLine("            options: {");
                writer.WriteLine("                responsive: true,");
                writer.WriteLine("                scales: {");
                writer.WriteLine("                    x: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '数据大小 (条目数)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    },");
                writer.WriteLine("                    y: {");
                writer.WriteLine("                        title: {");
                writer.WriteLine("                            display: true,");
                writer.WriteLine("                            text: '传输时间 (ms)'");
                writer.WriteLine("                        }");
                writer.WriteLine("                    }");
                writer.WriteLine("                }");
                writer.WriteLine("            }");
                writer.WriteLine("        });");
                writer.WriteLine("    </script>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
            
            Console.WriteLine($"图表已生成: {htmlPath}");
        }
        
        // 计算平均比率
        private double CalculateAverageRatio(List<double> numerator, List<double> denominator)
        {
            if (numerator.Count != denominator.Count || numerator.Count == 0)
                return 0;
                
            double sum = 0;
            for (int i = 0; i < numerator.Count; i++)
            {
                if (denominator[i] > 0)
                    sum += numerator[i] / denominator[i];
            }
            
            return sum / numerator.Count;
        }
        
        // 判断比率是否随着数据大小增加而增加
        private bool IsRatioIncreasing(List<double> numerator, List<double> denominator)
        {
            if (numerator.Count != denominator.Count || numerator.Count < 2)
                return false;
                
            // 计算比率
            List<double> ratios = new List<double>();
            for (int i = 0; i < numerator.Count; i++)
            {
                if (denominator[i] > 0)
                    ratios.Add(numerator[i] / denominator[i]);
                else
                    ratios.Add(0);
            }
            
            // 计算比率的线性回归斜率
            double xMean = (ratios.Count - 1) / 2.0;
            double yMean = ratios.Average();
            
            double numeratorSum = 0;
            double denominatorSum = 0;
            
            for (int i = 0; i < ratios.Count; i++)
            {
                numeratorSum += (i - xMean) * (ratios[i] - yMean);
                denominatorSum += Math.Pow(i - xMean, 2);
            }
            
            // 如果斜率为正，则表示比率随着数据大小增加而增加
            return denominatorSum > 0 && numeratorSum / denominatorSum > 0;
        }
                public static void Main(string[] args)
        {
            ProtobufVsJsonTest tester = new ProtobufVsJsonTest();
            tester.RunTest();

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}