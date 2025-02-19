#region

using System;
using System.Collections.Generic;
//using System.Net.WebSockets;
using BestHTTP.WebSocket;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

#endregion

public class NetManager : SingletonBase<NetManager>
{
    private const int reconnectMaxAttemps = 15;
    
    /**
     * 缓存的数据包
     */
    private readonly List<ISocketPackRequest> _cachePacks = new();
    
    private Timer _autoConnectTimeoutHandler;
    
    /**
     * 原始服务器地址
     */
    private string _host;
    
    /**
     * 下一个gid
     */
    private int _nextGid;
    
    /**
     * 请求的数据包
     */
    protected Dictionary<string, ISocketPackRequest> _requestPacks = new();
    
    /**
     * 当前连接
     */
    private WebSocket _socket;
    
    /**
     * 缓存数据的用户帐号
     */
    private string cacheUserId;
    
    /**
     * 心跳定时器 (多久发一次)
     */
    private Timer heartbeatTimerHandler;
    
    /**
     * 上次发送消息的时间
     */
    private long lastSendTime;
    
    private int reconnectAttemps;
    
    /**
     * 断线后是否自动重连
     */
    private bool reconnectOnCloseEnabled = true;
    
    private bool waitHearbeatResult;
    
    public void Init()
    {
        EventManager.Instance.On(EventId.ON_APPLICATION_PAUSE, OnApplicationPause);
        EventManager.Instance.On<long>(EventId.ON_APPLICATION_RESUME, OnApplicationResume);
    }
    
    public void Perge()
    {
        EventManager.Instance.Off(EventId.ON_APPLICATION_PAUSE, OnApplicationPause);
        EventManager.Instance.Off<long>(EventId.ON_APPLICATION_RESUME, OnApplicationResume);
    }
    
    public void Connect(string host)
    {
        //这里暂时用不到，直接走connect传进来的host
        if (GlobalData.Instance.GameIpAcc != null && GlobalData.Instance.UrlAccIdx == 1)
        {
            _host = GlobalData.Instance.GameIpAcc;
        }
        else
        {
            _host = host;
        }
        
        if (_socket != null && _socket.State == WebSocketStates.Open)
        {
            LogTool.LogWarning("(Socket) 尝试在一个活跃的连接上发起新的连接请求");
            return;
        }
        
        // _host = "wss://jl-ga-2024.jinglonggame.com:442/s1-wss";
        LogTool.Log("请求连接服务器" + _host);
        _socket = new WebSocket(new Uri(_host));
        _socket.StartPingThread = true;
        _socket.OnOpen += OnOpen;
        _socket.OnError += OnError;
        _socket.OnClosed += OnClosed;
        _socket.OnMessage += OnMessage;
        _socket.Open();
    }
    
    public void Reconnect()
    {
        if (_socket != null && _socket.State == WebSocketStates.Open)
        {
            LogTool.LogWarning("(Socket) 重连尝试在一个活跃的连接上发起新的连接请求");
            return;
        }
        
        if (reconnectAttemps > reconnectMaxAttemps)
        {
            return;
        }
        
        reconnectAttemps++;
        RemoveAutoConnectTimeout();
        StopHeartbeat();
        if (_socket != null)
        {
            _socket = null;
        }
        
        Connect(_host);
    }
    
    /**
     * 连接成功时
     */
    private void OnOpen(WebSocket ws)
    {
        reconnectAttemps = 0;
        RemoveAutoConnectTimeout();
        StartHeartbeat();
        EventManager.Instance.Emit(EventId.START_LOGIN);
    }
    
    private void OnMessage(WebSocket ws, string message)
    {
        ISocketPack pack = JsonTool.DeserializeObject<ISocketPack>(message);
        if (pack.c == (int)RequestId.NO_QUEUE_HEART)
        {
            LogTool.Log("(心跳逻辑) 返回");
        }
        
        if (pack.p != null && (long)pack.p == 1)
        {
            pack.d = SystemUtils.DecompressGzipBase64((string)pack.d);
        }
        
        if (pack.s == 3)
        {
            //  EVENT.emit(EventId.MVC_NET_TIPS, { type: GameDefine.MvcUIType.ShowTips, data: LOCAL.getNetMsgText(pack.d) });
        }
        
        if (pack.c != (int)ResponseId.ERROR_ID)
        {
            ISocketPackRequest req = null;
            if (pack.o != null)
            {
                req = _requestPacks[pack.o];
            }
            
            NetReciver.Instance.OnMessage(pack);
            if (req != null)
            {
                int idx = _cachePacks.IndexOf(req);
                if (idx >= 0)
                {
                    if (waitHearbeatResult && pack.c == (int)RequestId.NO_QUEUE_HEART)
                    {
                        waitHearbeatResult = false;
                    }
                    
                    _cachePacks.Remove(req);
                    _requestPacks.Remove(pack.o);
                }
                
                if (req.success != null)
                {
                    req.success.Invoke(pack);
                }
            }
        }
    }
    
    private void OnError(WebSocket ws, Exception exception)
    {
        StopHeartbeat();
        if (reconnectOnCloseEnabled)
        {
            AddAutoConnect();
        }
    }
    
    private void OnClosed(WebSocket ws, ushort code, string message)
    {
        StopHeartbeat();
        if (reconnectOnCloseEnabled)
        {
            AddAutoConnect();
        }
    }
    
    private void Close()
    {
        RemoveAutoConnectTimeout();
        StopHeartbeat();
        if (_socket != null)
        {
            _socket.Close();
            _socket = null;
        }
    }
    
    public void Dispose()
    {
        SetAutoReconnectOnClose(false);
        Close();
    }
    
    public void Send(RequestId id, JObject data = null, UnityAction<ISocketPack> success = null)
    {
        ISocketPack pack = new();
        pack.c = (int)id;
        pack.o = GetNextGid().ToString();
        pack.p = data != null ? data : new JObject();
        
        long now = TimeCenter.TimeStampMil;
        ISocketPackRequest req = new();
        req.pack = pack;
        req.success = success;
        req.reqTime = now;
        req.sendTime = now;
        req.sendNum = 1;
        
        _cachePacks.Add(req);
        _requestPacks.Add(req.pack.o, req);
        DoSend(req.pack);
    }
    
    private void DoSend(ISocketPack pack)
    {
        if (_socket != null)
        {
            if (_socket.State != WebSocketStates.Open)
            {
                return;
            }
            
            string text = JsonTool.SerializeObject(pack);
            _socket.Send(text);
            LogTool.Log("(Socket,req) 发送消息: " + text);
        }
        
        lastSendTime = TimeCenter.TimeStampMil;
    }
    
    private int GetNextGid()
    {
        int ret = _nextGid;
        _nextGid++;
        _nextGid = _nextGid % 100000000;
        return ret;
    }
    
    protected void AddAutoConnect()
    {
        if (_autoConnectTimeoutHandler != null)
        {
            return;
        }
        
        _autoConnectTimeoutHandler = TimerManager.Register(5, Reconnect);
    }
    
    private void RemoveAutoConnectTimeout()
    {
        if (_autoConnectTimeoutHandler != null)
        {
            TimerManager.RemoveTimer(_autoConnectTimeoutHandler);
            _autoConnectTimeoutHandler = null;
        }
    }
    
    private void StartHeartbeat()
    {
        StopHeartbeat();
        long now = TimeCenter.TimeStampMil;
        for (int i = 0; i < _cachePacks.Count; i++)
        {
            ISocketPackRequest req = _cachePacks[i];
            req.sendTime = now;
        }
        
        if (_host == null)
        {
            return;
        }
        
        heartbeatTimerHandler = TimerManager.Register(1, SendHeartPack, -1);
    }
    
    private void StopHeartbeat()
    {
        if (heartbeatTimerHandler != null)
        {
            TimerManager.RemoveTimer(heartbeatTimerHandler);
        }
    }
    
    private void SendHeartPack()
    {
        if (_socket == null)
        {
            return;
        }
        
        if (_socket.State != WebSocketStates.Open)
        {
            return;
        }
        
        long now = TimeCenter.TimeStampMil;
        bool isOverTime = false;
        bool needSendHeart = false;
        for (int i = 0; i < _cachePacks.Count; i++)
        {
            ISocketPackRequest req = _cachePacks[i];
            if (req != null)
            {
                if (waitHearbeatResult)
                {
                    if (req.pack.c == (int)RequestId.NO_QUEUE_HEART)
                    {
                        long offsetTime = now - (long)req.sendTime;
                        if (offsetTime >= SocketCore.IDLE_OVER * 1000)
                        {
                            isOverTime = true;
                            break;
                        }
                    }
                }
            }
        }
        
        if (!needSendHeart && !waitHearbeatResult)
        {
            needSendHeart = now - lastSendTime > SocketCore.IDLE_INTERVAL * 1000;
        }
        
        if (isOverTime)
        {
            LogTool.LogWarning("(心跳逻辑) 超时");
            Close();
            Reconnect();
        }
        
        if (needSendHeart)
        {
            waitHearbeatResult = true;
            Send(RequestId.NO_QUEUE_HEART, null, obj => { });
            LogTool.Log("(心跳逻辑) 发送");
        }
    }
    
    /**
     * 设置是否在断开连接时自动重连
     */
    public void SetAutoReconnectOnClose(bool e)
    {
        reconnectOnCloseEnabled = e;
    }
    
    /**
     * 登录成功之后检查历史数据
     */
    public void checkHistoryPacksAfterLogin(string uid)
    {
        if (uid != cacheUserId)
        {
            LogTool.Log("(NET) 清空历史消息 ");
            cacheUserId = uid;
            _cachePacks.Clear();
            _requestPacks.Clear();
        }
        
        trySendHistoryPacks();
    }
    
    /**
     * 尝试发送历史消息
     */
    private void trySendHistoryPacks()
    {
        // clearHistoryPacks();
        if (_cachePacks.Count <= 0)
        {
            return;
        }
        
        long now = TimeCenter.TimeStampMil;
        for (int i = 0; i < _cachePacks.Count; i++)
        {
            ISocketPackRequest req = _cachePacks[i];
            string text = JsonTool.SerializeObject(req.pack);
            LogTool.Log("(Net) 重发消息 " + text);
            DoSend(req.pack);
            ++req.sendNum;
            req.sendTime = now;
        }
        
        LogTool.LogWarning("(Net) 重发消息条数: " + _cachePacks.Count);
    }
    
    // /**
    //  * 清除历史消息
    //  */
    // private void clearHistoryPacks()
    // {
    //     if (_cachePacks.Count <= 0)
    //     {
    //         return;
    //     }
    //
    //     for (int i = _cachePacks.Count - 1; i >= 0; i--)
    //     {
    //         ISocketPackRequest req = _cachePacks[i];
    //     }
    // }
    
    private void OnApplicationPause()
    {
        SetAutoReconnectOnClose(false);
    }
    
    private void OnApplicationResume(long gap)
    {
        LogTool.Log("OnApplicationResume NET");
        SetAutoReconnectOnClose(true);
        StartHeartbeat();
        SendHeartPack();
    }
}