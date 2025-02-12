
#nullable enable

#region

using UnityEngine;
using UnityEngine.Events;

#endregion

public class SocketCore
{
    /**
     * 心跳间隔时间 (秒)
     */
    public static int IDLE_INTERVAL = 10;
    
    /**
     * 心跳超时时间 (秒)
     */
    public static int IDLE_OVER = 5;
    
    /**
     * 是否输出日志
     */
    public static bool LOG_ENABLED = Debug.isDebugBuild;
}

public class ISocketPackRequest
{
    /**
     * 数据包内容
     */
    public ISocketPack? pack;
    
    /**
     * 业务请求时间
     */
    public long? reqTime;
    
    /**
     * 响应时间
     */
    public long? resTime;
    
    /**
     * 发送次数
     */
    public int? sendNum;
    
    /**
     * 实际发送时间
     */
    public long? sendTime;
    
    /**
     * 成功回调函数
     */
    public UnityAction<ISocketPack>? success;
}

public class ISocketPack
{
    /**
     * 消息ID (发为两类：客户端请求服务器并应答时参考RequestId，服务器主动推送时参考ResponseId)
     */
    public int c;
    
    /**
     * 数据包的数据体内容 (ACK)
     */
    public object? d;
    
    /**
     * 服务器和客户端之间应答时约定的索引值, 客户端发起的请求，服务器在应答时会传回此值
     */
    public string? o;
    
    /**
     * 数据包的数据体内容 (REQ)
     */
    public object? p;
    
    /**
     * 服务器下发数据包时可能带的状态值
     */
    public int? s;
}