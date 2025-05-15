using SoulKnightProtocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientManager : BaseManager
{
    private Socket socket;
    private Socket udpClient;
    private IPEndPoint ipEndPoint;
    private EndPoint endPoint;
    private Byte[] buffer = new Byte[1024];
    private Thread aucThread;
    private Message message;
    private RequestManager requestManager;
    private String IPStr = "129.204.144.2";
    public RequestManager m_RequestManager => requestManager;
    public ClientManager(ClientFacade facade) : base(facade)
    {
        requestManager = new RequestManager(this);
    }
    public override void OnInit()
    {
        base.OnInit();
        message = new Message();
        InitSocket();
        InitUDP();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        message = null;
        CloseSocket();
    }
    private void InitSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            // 首先尝试连接远程服务器
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IPStr), 9999);
            IAsyncResult connResult = socket.BeginConnect(ep, null, null);
            connResult.AsyncWaitHandle.WaitOne(200);
            if (connResult.IsCompleted)
            {
                Debug.Log("成功连接到远程服务器: " + IPStr);
                BeginReceive();
            }
            else
            {
                // 远程连接超时，尝试连接本地服务器
                UnityEngine.Debug.Log("连接远程服务器超时，尝试连接本地服务器...");
                socket.Close(); // 关闭之前的连接尝试
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
                IAsyncResult localConnResult = socket.BeginConnect(localEp, null, null);
                localConnResult.AsyncWaitHandle.WaitOne(200);
                
                if (localConnResult.IsCompleted)
                {
                    IPStr = "127.0.0.1";
                    Debug.Log("成功连接到本地服务器");
                    BeginReceive();
                }
                else
                {
                    UnityEngine.Debug.Log("连接本地服务器也超时，无法建立连接");
                }
            }
        }
        catch (Exception e)
        {
            // 远程连接出错，尝试连接本地服务器
            UnityEngine.Debug.Log($"连接远程服务器失败: {e.Message}，尝试连接本地服务器...");
            try
            {
                socket.Close(); // 确保关闭之前的连接
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                IPEndPoint localEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
                IAsyncResult localConnResult = socket.BeginConnect(localEp, null, null);
                localConnResult.AsyncWaitHandle.WaitOne(200);
                
                if (localConnResult.IsCompleted)
                {
                    IPStr = "127.0.0.1";
                    Debug.Log("成功连接到本地服务器");
                    BeginReceive();
                }
                else
                {
                    UnityEngine.Debug.Log("连接本地服务器也超时，无法建立连接");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"连接本地服务器也失败: {ex.Message}");
            }
        }
    }
    private void CloseSocket()
    {
        if (socket.Connected && socket != null)
        {
            socket.Close();
        }
    }
    private void BeginReceive()
    {
        socket.BeginReceive(message.Buffer, message.StartIndex, message.RemainSize, SocketFlags.None, ReceiveCallback, null);
    }
    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int len = socket.EndReceive(result);
            // --- 添加日志 ---
            // --- 日志结束 ---

            if (len == 0)
            {
                Debug.LogWarning("[TCP Receive] Connection closed by remote host (received 0 bytes).");
                CloseSocket();
                return;
            }
            message.ReadBuffer(len, (pack) =>
            {
                requestManager.HandleResponse(pack);
            });
            BeginReceive();
        }
        catch(SocketException se) // 更具体地捕获 SocketException
        {
            Debug.LogError($"[TCP Receive Error] SocketException: {se.Message} (ErrorCode: {se.SocketErrorCode})");
            // 根据错误码判断是否需要关闭 Socket
            CloseSocket(); // 通常发生 Socket 错误后需要关闭
        }
        catch(Exception e)
        {
            Debug.LogError($"[TCP Receive Error] Unexpected Exception: {e}");
            CloseSocket(); // 发生未知严重错误，也关闭 Socket
            return; // 移除 return，因为 CloseSocket 已经处理
        }
    }


    private void InitUDP()
    {
        udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        UnityEngine.Debug.Log("UDP开始连接" + IPStr);
        ipEndPoint = new IPEndPoint(IPAddress.Parse(IPStr), 9998);
        endPoint = ipEndPoint;
        try
        {
            IAsyncResult udpResult = udpClient.BeginConnect(endPoint, null, null);
            udpResult.AsyncWaitHandle.WaitOne(2);
            if (udpResult.IsCompleted)
            {
                aucThread = new Thread(ReceiveMsg);
                aucThread.Start();
            }
            else
            {
                UnityEngine.Debug.Log("UDP服务器连接超时");
            }
        }
        catch
        {
            Debug.Log("UDP连接失败");
        }
    }
    private void ReceiveMsg()
    {
        Debug.Log("UDP开始接收" +endPoint.ToString());
        while (true)
        {
            int len = udpClient.ReceiveFrom(buffer, ref endPoint);
       
            MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);

            requestManager.HandleResponse(pack);
        }
    }
    public void Send(MainPack pack)
    {
        if (socket.Connected)
        {
            socket.Send(Message.ConvertToByteArray(pack));
        }
    }
    public void SendTo(MainPack pack)
    {
        pack.LoginPack.UserName = ModelContainer.Instance.GetModel<MemoryModel>().UserName;
        Byte[] sendBuff = Message.PackDataUDP(pack);
        udpClient.Send(sendBuff, sendBuff.Length, SocketFlags.None);
    }
}
