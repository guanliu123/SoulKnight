public class NetReciver : SingletonBase<NetReciver>
{
    private ReciverBase _reciver;
    
    public void Init(ReciverBase reciver)
    {
        _reciver = reciver;
        _reciver.Regist();
    }
    
    public void OnMessage(ISocketPack pack)
    {
        ResponseBase response = _reciver.Get((ResponseId)pack.c);
        if (response != null)
        {
            response.ResponseHandle(pack);
        }
        
        if (SocketCore.LOG_ENABLED)
        {
            if (pack.c != (int)RequestId.NO_QUEUE_HEART)
            {
                string text = JsonTool.SerializeObject(pack);
                LogTool.Log("(Socket) 接收消息 " + text);
            }
        }
    }
}