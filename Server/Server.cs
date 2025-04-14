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
        private List<Client> clientList = new List<Client>();
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
            
            // 初始化战斗管理器
            BattleManager.Instance.Initialize(this);
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
        }
        
        private void BeginAccept()
        {
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        
        private void AcceptCallback(IAsyncResult result)
        {
            Socket client = serverSocket.EndAccept(result);
            Console.WriteLine("增加了一个客户端");
            clientList.Add(new Client(client, this));
            BeginAccept();
        }
        
        public void RemoveClient(Client client)
        {
            clientList.Remove(client);
        }
        
        public Client GetClientByUserName(string user)
        {
            foreach (Client client in clientList)
            {
                if (client.userName == user)
                {
                    return client;
                }
            }
            return null;
        }
        
        // 添加获取客户端的方法
        public Client GetClientByID(int id)
        {
            foreach (Client client in clientList)
            {
                if (client.Id == id)
                {
                    return client;
                }
            }
            return null;
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

