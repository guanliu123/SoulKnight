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
            if (pack != null)
            {
                socket.Send(Message.ConvertToByteArray(pack));
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
