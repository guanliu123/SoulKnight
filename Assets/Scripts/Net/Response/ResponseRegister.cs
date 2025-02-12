
public class ResponseRegister : ReciverBase
{
    public override void Regist()
    {
        base.Regist();
        //网络连接成功
        Add(ResponseId.LOGIN, new LoginResponse());
        //this.Add(ResponseId.USER_DISCONNECT, new PushUserDisconnect());
        // Add(ResponseId.RESOURCE, new PushResponse.UserResourceResponse());
        // Add(ResponseId.PUSH_TEST, new PushResponse.PUSH_TEST());
        // Add(ResponseId.PUSH_PAY_TO_SERVER, new PushResponse.PayToServerResponse());
        // Add(ResponseId.UserDinoChanged, new PushResponse.UserDinoChangedServerResponse());
    }
}