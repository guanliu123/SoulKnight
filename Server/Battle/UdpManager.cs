using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using KnightServer;
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
                    string userName = pack.LoginPack.UserName;
                    Client client   = ClientManager.Instance.GetClientByUserName(userName);
                    if (client != null)
                    {
                        // pack.Str = client.IpAndPort;
                        if (client.remoteEp == null || client.remoteEp.ToString() != remoteEP.ToString())
                            client.remoteEp = remoteEP;
                    }else{
                        Console.WriteLine($"接收到来自 {remoteEP} 的消息无客户端");
                    }
                    messageHandler(pack); // <-- 问题可能发生在这里调用的方法内部
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
            catch (Exception ex) // 捕获所有其他异常
            {
                // --- 修改日志记录 ---
                Console.WriteLine($"--- UDP 接收处理时发生异常 ---");
                Console.WriteLine($"原始异常类型: {ex.GetType().FullName}");
                Console.WriteLine($"原始异常消息: {ex.Message}");
                // 检查并打印内部异常（这通常是 TargetInvocationException 的情况）
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"--- 内部异常 (根本原因) ---");
                    Console.WriteLine($"内部异常类型: {ex.InnerException.GetType().FullName}");
                    Console.WriteLine($"内部异常消息: {ex.InnerException.Message}");
                    Console.WriteLine($"内部异常堆栈跟踪:\n{ex.InnerException.StackTrace}");
                    Console.WriteLine($"--- 内部异常结束 ---");
                }
                else
                {
                    // 如果没有内部异常，打印外部异常的堆栈
                    Console.WriteLine($"异常堆栈跟踪:\n{ex.StackTrace}");
                }
                 Console.WriteLine($"--- UDP 异常结束 ---");
                // --- 日志记录修改结束 ---

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