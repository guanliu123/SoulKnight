using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SoulKnightProtocol;
using Battle; // 改回原来的命名空间

namespace KnightServer
{
    public class Server
    {
        private Socket serverSocket;
        private ControllerManager controllerManager;

        public Server(int port)
        {
            controllerManager = new ControllerManager(this);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(0);
            BeginAccept();

            // 初始化 UDPManager 并注册消息处理器
            UdpManager.Instance.RegisterHandler(HandleUdpMessage);
            HeartbeatManager.Instance.Start(this);

            // 初始化战斗管理器
            BattleManager.Instance.Initialize(this);

            Console.WriteLine("服务器已启动..."); // 添加启动日志
        }

        // 添加 UDP 消息处理方法
        private void HandleUdpMessage(MainPack pack)
        {
            // 将 UDP 消息转发给控制器管理器处理
            controllerManager.HandleUdpRequest(pack);
        }

        ~Server()
        {
            // 关闭 UDPManager
            UdpManager.Instance.Close();
            // 可以考虑在这里关闭服务器 Socket
            if (serverSocket != null)
            {
                serverSocket.Close();
            }
            Console.WriteLine("服务器已关闭..."); // 添加关闭日志
        }

        private void BeginAccept()
        {
            try
            {
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (ObjectDisposedException)
            {
                 Console.WriteLine("服务器 Socket 已关闭，停止接受新连接。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接受新连接时发生错误: {ex}");
                // 可以考虑是否需要重试或停止服务器
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(result);
                Console.WriteLine("接收到一个新连接...");
                Client newClient = new Client(clientSocket, this);
                
                // 不要使用 BeginDisconnect，它会主动断开连接
                // clientSocket.BeginDisconnect(false, DisconnectCallback, newClient);
                
                ClientManager.Instance.AddClient(newClient); // <-- 使用 ClientManager 添加
            }
            catch (ObjectDisposedException)
            {
                 Console.WriteLine("服务器 Socket 在接受连接期间被关闭。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理新连接时发生错误: {ex}");
            }
            finally
            {
                 // 无论成功或失败，都继续接受下一个连接（除非服务器已关闭）
                 BeginAccept();
            }
        }

        /// <summary>
        /// 从客户端管理器移除客户端
        /// </summary>
        /// <param name="client">要移除的客户端</param>
        public void RemoveClient(Client client)
        {
            // 调用 ClientManager 的方法
            ClientManager.Instance.RemoveClient(client); // <-- 使用 ClientManager 移除
        }

        /// <summary>
        /// 通过用户名从客户端管理器获取客户端
        /// </summary>
        /// <param name="user">用户名</param>
        /// <returns>找到的客户端，否则返回 null</returns>
        public Client GetClientByUserName(string user)
        {
            // 调用 ClientManager 的方法
            return ClientManager.Instance.GetClientByUserName(user); // <-- 从 ClientManager 获取
        }

        /// <summary>
        /// 通过 ID 从客户端管理器获取客户端
        /// </summary>
        /// <param name="id">客户端 ID</param>
        /// <returns>找到的客户端，否则返回 null</returns>
        public Client GetClientByID(int id)
        {
            // 调用 ClientManager 的方法
            return ClientManager.Instance.GetClientByID(id); // <-- 从 ClientManager 获取
        }

        // 战斗相关方法的代理，转发到 BattleManager
        public void AddPlayerToBattle(int playerId, int battleId)
        {
            BattleManager.Instance.AddPlayerToBattle(playerId, battleId);
        }

        public void RemovePlayerFromBattle(int playerId)
        {
            BattleManager.Instance.RemovePlayerFromBattle(playerId);
        }

        public List<int> GetBattlePlayers(int battleId)
        {
            return BattleManager.Instance.GetBattlePlayers(battleId);
        }

        public void ClearBattleData(int battleId)
        {
            BattleManager.Instance.ClearBattleData(battleId);
        }

        // 添加公共方法处理请求
        public void HandleRequest(MainPack pack, Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }
    }
}

