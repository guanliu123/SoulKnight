using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SoulKnightProtocol;
using Google.Protobuf;

namespace Tests
{
    public class TestClient
    {
        private string serverIP;
        private int serverPort;
        private TcpClient client;
        private NetworkStream stream;
        private bool isRunning = false;
        private Thread receiveThread;
        private Queue<MainPack> receiveQueue = new Queue<MainPack>();
        private ManualResetEvent waitHandle = new ManualResetEvent(false);
        private MainPack lastResponse = null;
        
        public int UserId { get; set; }
        public bool IsLoggedIn { get; set; }
        public int RoomId { get; set; }
        public bool IsConnected { get; private set; }
        
        public TestClient(string ip, int port)
        {
            this.serverIP = ip;
            this.serverPort = port;
            this.client = new TcpClient();
            this.IsLoggedIn = false;
            this.RoomId = 0;
            this.IsConnected = false;
        }
        
        public bool Connect()
        {
            try
            {
                client.Connect(serverIP, serverPort);
                stream = client.GetStream();
                isRunning = true;
                IsConnected = true;
                
                // 启动接收线程
                receiveThread = new Thread(ReceiveMessage);
                receiveThread.IsBackground = true;
                receiveThread.Start();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接服务器失败: {ex.Message}");
                IsConnected = false;
                return false;
            }
        }
        
        public void Disconnect()
        {
            isRunning = false;
            IsConnected = false;
            
            if (receiveThread != null)
                receiveThread.Join(1000);
                
            if (stream != null)
                stream.Close();
                
            if (client != null)
                client.Close();
        }
        
        public void Send(MainPack pack)
        {
            try
            {
                byte[] data = pack.ToByteArray();
                byte[] length = BitConverter.GetBytes(data.Length);
                
                stream.Write(length, 0, 4);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息失败: {ex.Message}");
            }
        }
        
        public MainPack SendAndWaitResponse(MainPack pack, int timeout = 10000)
        {
            lastResponse = null;
            waitHandle.Reset();
            
            Send(pack);
            
            // 等待响应，最多等待指定的超时时间
            if (waitHandle.WaitOne(timeout))
                return lastResponse;
            else
                return null;
        }
        
        public bool WaitForFrameSync(int frameId, int timeout = 5000)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
            {
                lock (receiveQueue)
                {
                    foreach (var pack in receiveQueue)
                    {
                        if (pack.ActionCode == ActionCode.BattlePushDowmAllFrameOpeartions && 
                            pack.BattleInfo != null && 
                            pack.BattleInfo.SelfOperation != null && 
                            pack.BattleInfo.SelfOperation.FrameId >= frameId)
                        {
                            return true;
                        }
                    }
                }
                Thread.Sleep(10);
            }
            return false;
        }
        
        private void ReceiveMessage()
        {
            byte[] lengthBytes = new byte[4];
            int length = 0;
            
            while (isRunning)
            {
                try
                {
                    // 读取消息长度
                    if (stream.Read(lengthBytes, 0, 4) <= 0)
                        break;
                        
                    length = BitConverter.ToInt32(lengthBytes, 0);
                    
                    // 读取消息内容
                    byte[] data = new byte[length];
                    int received = 0;
                    
                    while (received < length)
                    {
                        int count = stream.Read(data, received, length - received);
                        if (count <= 0)
                            break;
                            
                        received += count;
                    }
                    
                    // 解析消息
                    MainPack pack = MainPack.Parser.ParseFrom(data);
                    
                    // 处理响应
                    lastResponse = pack;
                    waitHandle.Set();
                    
                    // 将消息加入队列
                    lock (receiveQueue)
                    {
                        receiveQueue.Enqueue(pack);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"接收消息失败: {ex.Message}");
                    break;
                }
            }
            
            IsConnected = false;
        }
        
        public MainPack GetNextMessage()
        {
            lock (receiveQueue)
            {
                if (receiveQueue.Count > 0)
                    return receiveQueue.Dequeue();
                else
                    return null;
            }
        }
        
        public bool CheckForBroadcast(ActionCode actionCode, TimeSpan timeout)
        {
            DateTime end = DateTime.Now + timeout;
            while (DateTime.Now < end)
            {
                var msg = GetNextMessage(); // 假设你有这个方法获取收到的消息
                if (msg != null && msg.ActionCode == actionCode)
                    return true;
                Thread.Sleep(50);
            }
            return false;
        }
    }
}