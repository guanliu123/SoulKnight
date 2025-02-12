
public class ResponseBase
{
    public void ResponseHandle(ISocketPack pack)
    {
        Handler(pack);
    }
    
    protected virtual void Handler(ISocketPack pack)
    {
    }
}