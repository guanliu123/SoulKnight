using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SoulKnightProtocol;

namespace Battle
{
    public class UdpManager
    {
        private static UdpManager instance;
        public static UdpManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UdpManager();
                }
                return instance;
            }
        }
        
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private Action<MainPack> messageHandler;
        private bool isRunning = false;
        
        private UdpManager()
        {
            udpClient = new UdpClient(8889); // 使用固定端口
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            isRunning = true;
            
            // 开始接收消息
            StartReceive();
        }
        
        public void RegisterHandler(Action<MainPack> handler)
        {
            messageHandler = handler;
        }
        
        private void StartReceive()
        {
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] data = udpClient.EndReceive(ar, ref remoteEndPoint);
                
                // 处理接收到的数据
                if (data.Length > 0 && messageHandler != null)
                {
                    MainPack pack = MainPack.Parser.ParseFrom(data);
                    messageHandler(pack);
                }
                
                // 继续接收
                if (isRunning)
                {
                    StartReceive();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP接收错误: {ex.Message}");
                
                // 继续接收
                if (isRunning)
                {
                    StartReceive();
                }
            }
        }
        
        public void Send(MainPack pack, string ipAndPort)
        {
            try
            {
                string[] parts = ipAndPort.Split(':');
                if (parts.Length != 2)
                {
                    Console.WriteLine($"无效的IP和端口: {ipAndPort}");
                    return;
                }
                
                string ip = parts[0];
                int port = int.Parse(parts[1]);
                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                // 使用 Message 类的 ConvertToByteArray 方法
                byte[] data = Message.ConvertToByteArray(pack);
                
                udpClient.BeginSend(data, data.Length, endPoint, SendCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP发送错误: {ex.Message}");
            }
        }
        
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                udpClient.EndSend(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP发送完成错误: {ex.Message}");
            }
        }
        
        public void Close()
        {
            isRunning = false;
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
        }
    }
}