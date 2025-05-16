using System;
using System.Collections.Generic;
using System.Timers;  // 已经引入了这个命名空间
using SoulKnightProtocol;

namespace KnightServer
{
    public class HeartbeatManager
    {
        private static HeartbeatManager instance;
        public static HeartbeatManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new HeartbeatManager();
                return instance;
            }
        }
        
        // 修改这里，明确指定使用 System.Timers.Timer
        private System.Timers.Timer heartbeatTimer;
        private const int HEARTBEAT_INTERVAL = 10000; // 10秒发送一次心跳
        private const int MAX_MISSED_HEARTBEATS = 3; // 允许最多错过3次心跳
        
        private Server server;
        
        private HeartbeatManager()
        {
            // 这里也需要明确指定使用 System.Timers.Timer
            heartbeatTimer = new System.Timers.Timer(HEARTBEAT_INTERVAL);
            heartbeatTimer.Elapsed += SendHeartbeats;
            heartbeatTimer.AutoReset = true;
        }
        
        public void Start(Server server)
        {
            this.server = server;
            heartbeatTimer.Start();
            Console.WriteLine("心跳管理器已启动");
        }
        
        public void Stop()
        {
            heartbeatTimer.Stop();
            Console.WriteLine("心跳管理器已停止");
        }
        
        private void SendHeartbeats(object sender, ElapsedEventArgs e)
        {
            if (server == null) return;
            
            List<Client> clients = ClientManager.Instance.GetAllClients();
            
            foreach (Client client in clients)
            {
                // 检查客户端是否超时
                if ((DateTime.Now - client.LastHeartbeatTime).TotalMilliseconds > HEARTBEAT_INTERVAL * MAX_MISSED_HEARTBEATS)
                {
                    Console.WriteLine($"客户端 {client.userName ?? "未知用户"} 心跳超时，断开连接");
                    client.Close();
                    continue;
                }
                
                // 发送心跳包
                try
                {
                    MainPack heartbeatPack = new MainPack();
                    heartbeatPack.RequestCode = RequestCode.Heart;
                    heartbeatPack.ActionCode = ActionCode.Heartbeat;
                    client.Send(heartbeatPack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"向客户端 {client.userName ?? "未知用户"} 发送心跳包失败: {ex.Message}");
                }
            }
        }
    }
}