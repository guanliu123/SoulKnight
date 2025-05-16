using SoulKnightProtocol;
using System;

namespace KnightServer
{
    public class HeartController : BaseController
    {
        public HeartController(ControllerManager manager) : base(manager)
        {
            requestCode = RequestCode.Heart;
        }

        // 处理心跳请求
        public MainPack Heartbeat(Client client, MainPack pack)
        {
            // 更新客户端的最后心跳时间
            client.LastHeartbeatTime = DateTime.Now;
            
            return null;
        }
    }
}