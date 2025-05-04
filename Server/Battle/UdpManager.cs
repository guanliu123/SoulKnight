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
            udpClient = new UdpClient(9998); // 使用固定端口
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
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.EndReceive(ar, ref remoteEP);
                // 处理接收到的数据
                if (data.Length > 0 && messageHandler != null)
                {
                    MainPack pack = MainPack.Parser.ParseFrom(data);
                    
                    pack.Str = remoteEP.ToString();
                    messageHandler(pack);
                }
                
                // 继续接收
                if (isRunning)
                {
                    StartReceive();
                }
            }
            catch (ObjectDisposedException) // UdpClient 可能已被关闭
            {
                Console.WriteLine("UDP接收错误: UdpClient已被关闭。");
                isRunning = false; // 停止后续接收尝试
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP接收错误: {ex.Message}");
                
                // 发生其他错误时，也尝试继续接收（除非 UdpClient 已关闭）
                if (isRunning && udpClient != null)
                {
                    try
                    {
                        StartReceive();
                    }
                    catch (ObjectDisposedException) // 再次检查，以防在异常处理期间关闭
                    {
                         Console.WriteLine("UDP接收错误: UdpClient已被关闭。");
                         isRunning = false;
                    }
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