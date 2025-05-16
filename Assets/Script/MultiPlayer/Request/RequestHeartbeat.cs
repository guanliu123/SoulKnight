using SoulKnightProtocol;
using UnityEngine;
using UnityEngine.Events;

public class RequestHeartbeat : BaseRequest
{
    public RequestHeartbeat(RequestManager manager) : base(manager)
    {
        requestCode = RequestCode.Heart;
        actionCode = ActionCode.Heartbeat;
    }
    
    public override void OnResponse(MainPack pack)
    {
        // 收到服务器的心跳包，立即回复
        if (pack.ActionCode == ActionCode.Heartbeat)
        {
            // 回复心跳包
            SendHeartbeatResponse();
            Debug.Log("收到服务器心跳包并回复");
        }
    }
    
    // 回复服务器的心跳包
    private void SendHeartbeatResponse()
    {
        MainPack mainPack = new MainPack();
        mainPack.RequestCode = RequestCode.Heart;
        mainPack.ActionCode = ActionCode.Heartbeat;
        base.SendRequest(mainPack);
    }
}