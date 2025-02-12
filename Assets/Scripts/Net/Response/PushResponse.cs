
#region

using Newtonsoft.Json.Linq;

#endregion

public class PushResponse
{
    public class PUSH_TEST : ResponseBase
    {
        protected override void Handler(ISocketPack pack)
        {
            base.Handler(pack);
            JObject data = JObject.Parse((string)pack.d);
        }
    }
    
    // public class PayToServerResponse : ResponseBase
    // {
    //     protected override void Handler(ISocketPack pack)
    //     {
    //         base.Handler(pack);
    //         JObject data = JObject.Parse((string)pack.d);
    //         EventManager.Instance.Emit(EventId.PAY_TO_SERVER_RESPONSE, data);
    //     }
    // }
    //
    // public class UserDinoChangedServerResponse : ResponseBase
    // {
    //     protected override void Handler(ISocketPack pack)
    //     {
    //         base.Handler(pack);
    //         JObject data = JObject.Parse((string)pack.d);
    //         
    //         DinosaurManager.Instance.UpdateDinosaurWithServerData(data.ToObject<TUserDino>());
    //     }
    // }
    //
    // public class UserResourceResponse : ResponseBase
    // {
    //     protected override void Handler(ISocketPack pack)
    //     {
    //         base.Handler(pack);
    //         JObject data = JObject.Parse((string)pack.d);
    //         
    //         ItemResManager.Instance.UpdateResource(data.ToObject<TResources>());
    //     }
    // }
}