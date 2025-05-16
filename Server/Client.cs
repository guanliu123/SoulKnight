using SoulKnightProtocol;
using System;
using System.Net;
using System.Net.Sockets;
using Battle;
using System.Timers;

namespace KnightServer
{
    public class Client
    {
        private Socket socket;
        private Server server;
        public Server Server => server;
        private Message message;
        private UserDao userDao;
        public UserDao m_UserDao => userDao;
        private Room? room = null;
        public Room Room => room;
        public EndPoint remoteEp;
        public string userName;
        public string PlayerType;
        
        // 添加 Id 属性
        public int Id { get; set; }
        
        // 添加最后心跳时间属性
        public DateTime LastHeartbeatTime { get; set; }
        
        // 在 Client 类中添加以下属性
        public Socket ClientSocket { get; private set; }
        
        // 并在构造函数中初始化
        // 在 Client 类中添加心跳检测相关字段
        private System.Timers.Timer heartbeatTimer;
        private DateTime lastReceiveTime;
        private const int HEARTBEAT_INTERVAL = 10000; // 10秒检测一次
        private const int TIMEOUT_SECONDS = 30; // 30秒无响应视为断开
        
        public Client(Socket socket, Server server)
        {
            this.socket = socket;
            this.server = server;
            this.ClientSocket = socket;
            
            // 初始化最后心跳时间
            LastHeartbeatTime = DateTime.Now;
            
            // 增强 KeepAlive 设置
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            // 设置 TCP 超时检测
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
            
            // 设置接收超时
            socket.ReceiveTimeout = 30000; // 30秒
            
            userDao = new UserDao();
            message = new Message();
            Console.WriteLine("新客户端连接已建立");
            // 初始化心跳检测
            lastReceiveTime = DateTime.Now;
            heartbeatTimer = new System.Timers.Timer(HEARTBEAT_INTERVAL);
            heartbeatTimer.Elapsed += CheckHeartbeat;
            heartbeatTimer.AutoReset = true;
            heartbeatTimer.Start();
            
            BeginReceive();
        }
        
        private void BeginReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }
        
        private void CheckHeartbeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // 检查 socket 是否为 null
                if (socket == null)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} 的 socket 为 null，关闭连接");
                    Close();
                    return;
                }
                
                // 使用 LastHeartbeatTime 作为客户端活跃的唯一依据
                if ((DateTime.Now - LastHeartbeatTime).TotalSeconds > TIMEOUT_SECONDS)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} 心跳超时且连接已断开，关闭连接");
                    Close();
                }
                else if ((DateTime.Now - LastHeartbeatTime).TotalSeconds > TIMEOUT_SECONDS)
                {
                    // 如果只是超时但连接仍然存在，可以尝试发送心跳包或记录日志
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} 心跳超时但连接仍存在，继续监控");
                    
                    // 可选：尝试发送心跳包检测连接
                    try
                    {
                        // 创建一个简单的心跳包
                        MainPack heartbeatPack = new MainPack();
                        heartbeatPack.ActionCode = ActionCode.Heartbeat;
                        Send(heartbeatPack);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"发送心跳包失败，可能连接已断开: {ex.Message}");
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"心跳检测发生异常: {ex.Message}");
            }
        }
        
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                // 不再更新 lastReceiveTime，只在心跳包处理中更新 LastHeartbeatTime
                
                if (socket == null)
                {
                    Console.WriteLine("Socket 为 null，客户端已断开");
                    Close();
                    return;
                }
                
                if (!socket.Connected)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} 已断开连接 (socket.Connected = false)");
                    Close();
                    return;
                }
                
                int len = 0;
                try
                {
                    len = socket.EndReceive(result);
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} Socket已被释放");
                    Close();
                    return;
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} EndReceive时发生Socket异常: {se.Message}, 错误码: {se.SocketErrorCode}");
                    Close();
                    return;
                }
                
                if (len == 0)
                {
                    Console.WriteLine($"客户端 {userName ?? "未知用户"} 已断开连接 (接收到0字节)");
                    Close();
                    return;
                }
                
                message.ReadBuffer(len, (pack) =>
                {
                    // 使用 Server 的公共方法处理请求
                    server.HandleRequest(pack, this);
                });
                
                BeginReceive();
            }
            catch (SocketException se)
            {
                Console.WriteLine($"客户端 {userName ?? "未知用户"} 接收数据时发生Socket异常: {se.Message}");
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Close();
            }
        }
        
        // 移除 CheckHeartbeat 方法和相关的心跳定时器
        
        // 将 Close 方法改为 public
        public void Close()
        {
            try
            {
                // 停止心跳检测
                if (heartbeatTimer != null)
                {
                    heartbeatTimer.Stop();
                    heartbeatTimer.Dispose();
                    heartbeatTimer = null;
                }
                // 先从管理器中移除客户端
                server.RemoveClient(this);
                if (room != null)
                {
                    room.RemovePlayer(this);
                    if (room.IsEmpty()){
                        Console.WriteLine($"房间 {room.m_RoomName} 已空，已移除");
                        RoomManager.Instance.RemoveRoom(room.m_RoomName);
                    }
                }
                // 然后关闭 socket
                if (socket != null)
                {
                    socket.Close();
                }
                
                userDao.CloseConn();
                Console.WriteLine($"关闭了客户端 {userName ?? "未知用户"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭客户端时发生异常: {ex.Message}");
            }
        }
        
        public void Send(MainPack pack)
        {
            // 检查 socket 是否为 null 或已断开连接
            if (socket == null || !socket.Connected)
            {
                Close();
                return;
            }
        
            if (pack != null)
            {
                try
                {
                    socket.Send(Message.ConvertToByteArray(pack));
                }
                catch (ObjectDisposedException ode) // 捕获 Socket 已关闭的异常
                {
                    Console.WriteLine($"发送消息给客户端 {userName ?? Id.ToString()} 时出错：Socket已被释放。\n{ode}");
                    // 可以考虑在这里再次调用 Close() 确保资源清理，但要小心循环调用
                    Close();    
                }
                catch (SocketException se) // 捕获其他 Socket 相关异常
                {
                    Console.WriteLine($"发送消息给客户端 {userName ?? Id.ToString()} 时发生 Socket 异常：\n{se}");
                    // 根据 SocketException 的错误码可以判断具体原因，例如连接重置
                    Close(); // 通常发生 SocketException 也意味着连接有问题，可以关闭
                }
                catch (Exception ex) // 捕获其他未知异常
                {
                    Console.WriteLine($"发送消息给客户端 {userName ?? Id.ToString()} 时发生未知异常：\n{ex}");
                    Close(); // 发生未知异常，也关闭连接以防万一
                }
            }
        }
        
        public void SendTo(MainPack pack)
        {
            if (remoteEp == null)
            {
                 return;
            }
            byte[] buff = Message.PackDataUDP(pack);
            // 修改为使用 UdpManager
            UdpManager.Instance.Send(pack, remoteEp.ToString());
        }
        
        
        public void SetRoom(Room room)
        {
            this.room = room!;
        }
        
        // 修改 UDP 相关代码
        public void SendUDP(MainPack pack)
        {
            // 使用 UdpManager 替代 UDPServer
            if (remoteEp != null)
            {
                UdpManager.Instance.Send(pack, remoteEp.ToString());
            }
        }
    }
}
