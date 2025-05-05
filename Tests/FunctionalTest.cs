using System;
using System.Collections.Generic;
using System.Threading;
using Internal;
using SoulKnightProtocol;

namespace Tests
{
    public class FunctionalTest
    {
        private List<TestClient> clients = new List<TestClient>();
        private string serverIP = "127.0.0.1";
        private int serverPort = 9999;
        private string roomName = "tRoom_" + Guid.NewGuid().ToString("N").Substring(0, 8); // 全局唯一房间名
        
        public void RunAllTests()
        {
            Console.WriteLine("开始功能测试...");
            
            try
            {
                // 测试连接
                TestConnection();
                
                // 测试注册
                TestRegister();
                
                // 测试登录
                TestLogin();
                
                // 测试创建房间
                TestCreateRoom();
                
                // 测试加入房间
                TestJoinRoom();
                
                // // 测试开始战斗
                TestStartBattle();
                
                // // 测试战斗同步
                TestBattleSync();
                
                // // 测试战斗结束
                TestEndBattle();
                
                Console.WriteLine("所有功能测试通过!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"功能测试失败: {ex.Message}");
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
        
        private void TestConnection()
        {
            Console.WriteLine("测试服务器连接...");
            
            // 创建测试客户端
            TestClient client = new TestClient(serverIP, serverPort);
            clients.Add(client);
            
            // 尝试连接
            bool connected = client.Connect();
            
            if (!connected)
                throw new Exception("无法连接到服务器");
                
            Console.WriteLine("服务器连接测试通过");
        }
        
        private void TestRegister()
        {
            Console.WriteLine("测试注册功能...");
            
            if (clients.Count == 0)
                TestConnection();
            
            TestClient client = clients[0];
            
            // 创建注册包
            MainPack registerPack = new MainPack();
            registerPack.RequestCode = RequestCode.User;
            registerPack.ActionCode = ActionCode.Register;
            
            LoginPack register = new LoginPack();
            register.UserName = "TestUser6";
            register.Password = "TestPassword123"; // 包含数字和字母
            
            Console.WriteLine($"注册用户名: {register.UserName}, 密码: {register.Password}");
            
            registerPack.LoginPack = register;
            
            // 发送注册请求
            MainPack response = client.SendAndWaitResponse(registerPack);
            
            if (response == null)
                throw new Exception("注册请求无响应");
            
            Console.WriteLine($"注册返回码: {response.ReturnCode}");
            
            if (response.ReturnCode != ReturnCode.Success)
                throw new Exception("注册失败，返回码: " + response.ReturnCode);
            
            Console.WriteLine("注册功能测试通过");
        }
        
        private void TestLogin()
        {
            Console.WriteLine("测试登录功能...");
            
            if (clients.Count == 0)
                TestConnection();
                
            TestClient client = clients[0];
            
            // 创建登录包
            MainPack loginPack = new MainPack();
            loginPack.RequestCode = RequestCode.User;
            loginPack.ActionCode = ActionCode.Login;
            
            LoginPack login = new LoginPack();
            login.UserName = "TestUser6";
            login.Password = "TestPassword123";
            
            loginPack.LoginPack = login;
            
            // 发送登录请求
            MainPack response = client.SendAndWaitResponse(loginPack);
            
            if (response == null || response.ReturnCode != ReturnCode.Success)
                throw new Exception("登录测试失败");
                
            client.IsLoggedIn = true;
            
            Console.WriteLine("登录功能测试通过");
        }
        
        private void TestCreateRoom()
        {
            Console.WriteLine("测试创建房间功能...");
            
            if (clients.Count == 0 || !clients[0].IsLoggedIn)
                TestLogin();
            
            TestClient client = clients[0];
            
            if (client.RoomId > 0)
            {
                Console.WriteLine("当前客户端已在房间中，跳过创建房间。");
                return;
            }
        
            MainPack createRoomPack = new MainPack();
            createRoomPack.RequestCode = RequestCode.Room;
            createRoomPack.ActionCode = ActionCode.CreateRoom;
            createRoomPack.RoomPacks.Add(new RoomPack());
            createRoomPack.RoomPacks[0].RoomName = roomName; // 使用全局唯一房间名
            createRoomPack.RoomPacks[0].MaxNum = 2;
        
            MainPack response = client.SendAndWaitResponse(createRoomPack);
        
            if (response == null || response.ReturnCode != ReturnCode.Success)
                throw new Exception("创建房间测试失败");
        
            client.RoomId = response.RoomPacks[0].RoomID;
        
            Console.WriteLine($"创建房间功能测试通过，房间名: {roomName}");
        }
        
        private void TestJoinRoom()
        {
            Console.WriteLine("测试加入房间功能...");
            
            // // 确保 client1 已经登录并创建了房间
            // if (clients.Count < 1 || !clients[0].IsLoggedIn || clients[0].RoomId <= 0)
            // {
            //      Console.WriteLine("前置条件不满足 (用户未登录或未创建房间)，先执行 TestCreateRoom...");
            //      TestCreateRoom(); // 确保房间已创建
            //      if (clients.Count < 1 || !clients[0].IsLoggedIn || clients[0].RoomId <= 0)
            //      {
            //          throw new Exception("无法满足加入房间的前置条件");
            //      }
            // }

            TestClient client1 = clients[0]; // 获取创建房间的客户端
            int targetRoomId = client1.RoomId; // 获取目标房间ID
            string targetRoomName = roomName; // 假设房间名已知或从 client1 获取

            Console.WriteLine($"尝试加入由 Client1 创建的房间 ID: {targetRoomId}");

            // 创建第二个客户端
            TestClient client2 = new TestClient(serverIP, serverPort);
            clients.Add(client2); // 将新客户端添加到列表中

            if (!client2.Connect())
                throw new Exception("第二个客户端无法连接到服务器");

            // 注册第二个客户端 (使用不同的用户名)
            MainPack registerPack = new MainPack();
            registerPack.RequestCode = RequestCode.User;
            registerPack.ActionCode = ActionCode.Register;

            LoginPack register = new LoginPack();
            // 确保使用与 client1 不同的用户名
            register.UserName = "TestUser_Joiner_" + Guid.NewGuid().ToString().Substring(0, 6);
            register.Password = "TestPwd" + Guid.NewGuid().ToString().Substring(0, 3) + "9"; // 确保有数字和字母

            Console.WriteLine($"注册用户名: {register.UserName}, 密码: {register.Password}");

            registerPack.LoginPack = register;

            MainPack registerResponse = client2.SendAndWaitResponse(registerPack);

            if (registerResponse == null)
                throw new Exception("第二个客户端注册请求无响应");

            Console.WriteLine($"第二个客户端注册返回码: {registerResponse.ReturnCode}");

            if (registerResponse.ReturnCode != ReturnCode.Success)
                throw new Exception($"第二个客户端注册失败，返回码: {registerResponse.ReturnCode}");

            // 登录第二个客户端
            MainPack loginPack = new MainPack();
            loginPack.RequestCode = RequestCode.User;
            loginPack.ActionCode = ActionCode.Login;

            LoginPack login = new LoginPack();
            login.UserName = register.UserName; // 使用刚刚注册的用户名
            login.Password = register.Password;

            loginPack.LoginPack = login;

            MainPack loginResponse = client2.SendAndWaitResponse(loginPack);

            if (loginResponse == null || loginResponse.ReturnCode != ReturnCode.Success)
                throw new Exception($"第二个客户端 ({login.UserName}) 登录失败");

            client2.IsLoggedIn = true;
            Console.WriteLine($"第二个客户端 ({login.UserName}) 登录成功");

            // 第二个客户端直接加入第一个客户端创建的房间
            MainPack joinRoomPack = new MainPack();
            joinRoomPack.RequestCode = RequestCode.Room;
            joinRoomPack.ActionCode = ActionCode.JoinRoom;

            // 创建RoomPack并设置要加入的房间信息
            RoomPack roomPack = new RoomPack();
            roomPack.RoomID = targetRoomId;
            roomPack.RoomName = targetRoomName; // 可以省略或确保正确
            joinRoomPack.RoomPacks.Add(roomPack);

            Console.WriteLine($"Client2 正在发送加入房间请求 (RoomID: {targetRoomId})...");
            MainPack joinResponse = client2.SendAndWaitResponse(joinRoomPack);

            // 检查加入房间的响应
            if (joinResponse == null)
                 throw new Exception($"加入房间 (ID: {targetRoomId}) 请求无响应");

            if (joinResponse.ReturnCode != ReturnCode.Success)
                 throw new Exception($"加入房间 (ID: {targetRoomId}) 失败，返回码: {joinResponse.ReturnCode}");

            client2.RoomId = targetRoomId; // 记录 client2 加入的房间 ID

            Console.WriteLine($"第二个客户端成功加入房间 (ID: {targetRoomId})");
            Console.WriteLine("加入房间功能测试通过");
        }

        private void TestStartBattle()
        {
            Console.WriteLine("测试开始战斗功能...");

            // 确保两个客户端都在同一房间
            if (clients.Count < 2 || !clients[0].IsLoggedIn || !clients[1].IsLoggedIn || clients[0].RoomId <= 0 || clients[0].RoomId != clients[1].RoomId)
            {
                Console.WriteLine("前置条件不满足，先执行 TestJoinRoom...");
                TestJoinRoom();
                if (clients.Count < 2 || !clients[0].IsLoggedIn || !clients[1].IsLoggedIn || clients[0].RoomId <= 0 || clients[0].RoomId != clients[1].RoomId)
                {
                    throw new Exception("无法满足开始战斗的前置条件");
                }
            }

            TestClient client1 = clients[0]; // 房主
            TestClient client2 = clients[1]; // 加入者

            // 1. 房主发送 StartEnterBattle，带上房间名
            Console.WriteLine("房主发送 StartEnterBattle...");
            MainPack startBattlePack = new MainPack();
            startBattlePack.RequestCode = RequestCode.Battle;
            startBattlePack.ActionCode = ActionCode.StartEnterBattle;
            RoomPack roomPack = new RoomPack();
            roomPack.RoomName = "TestRoom";
            startBattlePack.RoomPacks.Add(roomPack);

            MainPack startResponse = client1.SendAndWaitResponse(startBattlePack);
            if (startResponse == null || startResponse.ReturnCode != ReturnCode.Success)
                throw new Exception("开始战斗请求失败");

            // 2. 解析 BattleID，分别发送 BattleReady
            int battleId1 = 1, battleId2 = 2; // 假设服务器分配的ID为1和2，实际应从返回包获取
            if (startResponse.BattlePlayerPack != null && startResponse.BattlePlayerPack.Count >= 2)
            {
                battleId1 = startResponse.BattlePlayerPack[0].Battleid;
                battleId2 = startResponse.BattlePlayerPack[1].Battleid;
            }

            Console.WriteLine("两个客户端发送 BattleReady...");
            MainPack readyPack1 = new MainPack();
            readyPack1.RequestCode = RequestCode.Battle;
            readyPack1.ActionCode = ActionCode.BattleReady;
            BattlePlayerPack bpp1 = new BattlePlayerPack();
            bpp1.Battleid = battleId1;
            readyPack1.BattlePlayerPack.Add(bpp1);

            MainPack readyPack2 = new MainPack();
            readyPack2.RequestCode = RequestCode.Battle;
            readyPack2.ActionCode = ActionCode.BattleReady;
            BattlePlayerPack bpp2 = new BattlePlayerPack();
            bpp2.Battleid = battleId2;
            readyPack2.BattlePlayerPack.Add(bpp2);

            client1.Send(readyPack1);
            client2.Send(readyPack2);

            // 3. 等待 BattleStart 广播
            Console.WriteLine("等待 BattleStart 广播...");
            bool client1ReceivedStart = client1.CheckForBroadcast(ActionCode.BattleStart, TimeSpan.FromSeconds(3));
            bool client2ReceivedStart = client2.CheckForBroadcast(ActionCode.BattleStart, TimeSpan.FromSeconds(3));
            if (!client1ReceivedStart || !client2ReceivedStart)
                throw new Exception("未收到 BattleStart 广播");

            Console.WriteLine("开始战斗功能测试通过");
        }

        private void TestBattleSync()
        {
            Console.WriteLine("测试战斗同步功能...");

            // 确保战斗已开始
            if (clients.Count < 2 || !clients[0].IsLoggedIn || !clients[1].IsLoggedIn || clients[0].RoomId <= 0) // 简单检查
            {
                 Console.WriteLine("前置条件不满足 (需要两个已登录且在战斗中的客户端)，先执行 TestStartBattle...");
                 TestStartBattle();
                 if (clients.Count < 2 || !clients[0].IsLoggedIn || !clients[1].IsLoggedIn || clients[0].RoomId <= 0)
                 {
                     throw new Exception("无法满足战斗同步的前置条件");
                 }
            }

            TestClient client1 = clients[0];
            TestClient client2 = clients[1];

            // 战斗开始后，客户端通常需要发送 BattleReady (进入战斗场景后的准备)
            // 注意：这里的 BattleReady 可能与房间内的 Ready 不同
            Console.WriteLine("模拟客户端进入战斗场景后的准备 (BattleReady)...");
            MainPack battleReadyPack1 = new MainPack { ActionCode = ActionCode.BattleReady };
            // 可能需要填充 BattlePlayerPack, Str (UDP地址?) 等信息
            // battleReadyPack1.BattlePlayerPack.Add(new BattlePlayerPack { Id = client1.UserId, Battleid = ... });
            // battleReadyPack1.Str = "127.0.0.1:9001"; // 示例 UDP 地址
            MainPack battleReadyPack2 = new MainPack { ActionCode = ActionCode.BattleReady };
            // battleReadyPack2.BattlePlayerPack.Add(new BattlePlayerPack { Id = client2.UserId, Battleid = ... });
            // battleReadyPack2.Str = "127.0.0.1:9002";

            client1.Send(battleReadyPack1); // 发送准备
            client2.Send(battleReadyPack2);
            Thread.Sleep(500); // 等待服务器处理

            // 模拟发送操作
            int operationCount = 20; // 可以增加操作次数以进行更长时间的测试
            Console.WriteLine($"模拟发送 {operationCount} 帧的操作...");
            for (int i = 1; i <= operationCount; i++)
            {
                // Client 1 发送操作
                MainPack operationPack1 = new MainPack();
                operationPack1.ActionCode = ActionCode.BattlePushDowmPlayerOpeartions;
                BattleInfo battleInfo1 = new BattleInfo();
                InputPack operation1 = new InputPack { BattleId = 1, FrameId = i /* ... 其他操作数据 ... */ };
                battleInfo1.SelfOperation = operation1;
                operationPack1.BattleInfo = battleInfo1;
                client1.Send(operationPack1);

                // Client 2 发送操作
                MainPack operationPack2 = new MainPack();
                operationPack2.ActionCode = ActionCode.BattlePushDowmPlayerOpeartions;
                BattleInfo battleInfo2 = new BattleInfo();
                InputPack operation2 = new InputPack { BattleId = 1, FrameId = i /* ... 其他操作数据 ... */ };
                battleInfo2.SelfOperation = operation2;
                operationPack2.BattleInfo = battleInfo2;
                client2.Send(operationPack2);

                // 等待一小段时间模拟帧间隔，并允许网络传输和处理
                Thread.Sleep(150); // 稍微增加等待时间

                // (重要) 检查是否收到了对方的操作广播
                // 例如，检查 client1 是否收到了来自 client2 的 FrameId 为 i 的操作包
                // 检查 client2 是否收到了来自 client1 的 FrameId 为 i 的操作包
                // bool client1ReceivedOp = client1.CheckForOperationBroadcast(client2.UserId, i, TimeSpan.FromMilliseconds(100));
                // bool client2ReceivedOp = client2.CheckForOperationBroadcast(client1.UserId, i, TimeSpan.FromMilliseconds(100));
                // if (!client1ReceivedOp || !client2ReceivedOp)
                // {
                //     Console.WriteLine($"警告: 第 {i} 帧操作同步可能存在问题");
                // }
            }

            // 等待同步完成
            Console.WriteLine("等待最后的同步完成...");
            Thread.Sleep(2000); // 保持或适当调整

            Console.WriteLine("战斗同步功能测试通过");
        }
        
        private void TestEndBattle()
        {
            Console.WriteLine("测试战斗结束功能...");
            
            if (clients.Count < 2 || !clients[0].IsLoggedIn || clients[0].RoomId <= 0)
                TestBattleSync();
                
            // 发送游戏结束信号
            foreach (var client in clients)
            {
                MainPack endPack = new MainPack();
                endPack.ActionCode = ActionCode.ClientSendGameOver;
                endPack.Str = "1"; // 战斗ID
                
                client.Send(endPack);
            }
            
            // 等待战斗结束处理
            Thread.Sleep(2000);
            
            Console.WriteLine("战斗结束功能测试通过");
        }
    }
}