using SoulKnightProtocol;
using System;
using System.Net;
using System.Net.Sockets;
using Battle;

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
        
        public Client(Socket socket, Server server)
        {
            this.socket = socket;
            this.server = server;
            userDao = new UserDao();
            message = new Message();
            BeginReceive();
        }
        
        private void BeginReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }
        
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                if (socket == null || socket.Connected == false) return;
                int len = socket.EndReceive(result);
                if (len == 0)
                {
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Close();
            }
        }
        
        public void Send(MainPack pack)
        {
            // 检查 socket 是否为 null 或已断开连接
            if (socket == null || !socket.Connected)
            {
                // Console.WriteLine($"尝试向已断开或未初始化的客户端 {userName ?? Id.ToString()} 发送消息，已跳过。");
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
                    // Close();
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
            if (remoteEp == null) return;
            byte[] buff = Message.PackDataUDP(pack);
            // 修改为使用 UdpManager
            UdpManager.Instance.Send(pack, remoteEp.ToString());
        }
        
        private void Close()
        {
            if (room != null)
            {
                room.RemovePlayer(this);
            }
            server.RemoveClient(this);
            socket.Close();
            userDao.CloseConn();
            Console.WriteLine("关闭了一个客户端");
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
