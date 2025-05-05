using Internal;
using SoulKnightProtocol;
using System.Reflection;
using Battle;

namespace KnightServer
{
    public class ControllerManager
    {
        private Dictionary<RequestCode, BaseController> controllerDic = new Dictionary<RequestCode, BaseController>();
        private Server server;
        public Server m_Server => server;
        public ControllerManager(Server server)
        {
            controllerDic.Add(RequestCode.User, new UserController(this));
            controllerDic.Add(RequestCode.Room, new RoomController(this));
            controllerDic.Add(RequestCode.Game, new GameController(this));
            controllerDic.Add(RequestCode.Battle, new BattleController(this));
            this.server = server;
        }
        
        // 现有的请求处理方法
        public void HandleRequest(MainPack pack, Client client, bool isUDP = false)
        {
            if (controllerDic.TryGetValue(pack.RequestCode, out BaseController controller))
            {
                string methodName = pack.ActionCode.ToString();
                MethodInfo methodInfo = controller.GetType().GetMethod(methodName);
                if (methodInfo == null)
                {
                    Console.WriteLine("没有找到对应的Action: " + methodName + " 来处理请求");
                }
                if (isUDP)
                {
                    methodInfo.Invoke(controller, new object[] { client, pack });

                }
                else
                {
                    object ret = methodInfo.Invoke(controller, new object[] { client, pack });
                    if (ret != null)
                    {
                        client.Send(ret as MainPack);
                    }
                }

            }
            else
            {

            }
        }
        
        // 添加处理 UDP 消息的方法
        public void HandleUdpRequest(MainPack pack)
        {
            // 根据 IP:端口 查找对应的客户端
            string ipPort = pack.Str;
            Client client = null;
            
            // 如果是战斗相关的请求，直接转发给战斗管理器
            if (pack.RequestCode == RequestCode.Battle)
            {
                // 战斗相关的 UDP 消息由 BattleManager 处理
                HandleBattleRequest(pack);
                return;
            }
            
            // 其他类型的 UDP 消息，尝试找到对应的控制器处理
            if (controllerDic.TryGetValue(pack.RequestCode, out BaseController controller))
            {
                string methodName = pack.ActionCode.ToString();
                MethodInfo methodInfo = controller.GetType().GetMethod(methodName);
                if (methodInfo == null)
                {
                    Console.WriteLine("没有找到对应的Action: " + methodName + " 来处理UDP请求");
                    return;
                }
                string userName = pack.LoginPack.UserName;
                client  = server.GetClientByUserName(userName);
                if (client == null){
                    Console.WriteLine($"未找到对应IP:端口的客户端: {userName} {methodName}");
                    return;
                }
                HandleRequest(pack, client, false);
                // 如果找到了客户端，则调用对应的方法
                if (client != null)
                {
                    object ret = methodInfo.Invoke(controller, new object[] { client, pack });
                    if (ret != null)
                    {
                        client.Send(ret as MainPack);
                    }
                }
                else
                {
                    // 如果没有找到客户端，可能需要特殊处理
                    Console.WriteLine($"未找到对应IP:端口的客户端: {ipPort} {methodName}");
                }
            }
            else
            {
                Console.WriteLine($"未找到对应的控制器处理UDP请求: {pack.RequestCode}");
            }
        }
        
        // 将嵌套函数移出来作为独立方法
        private void HandleBattleRequest(MainPack pack)
        {
            // 使用 BattleManager 而不是 Server.Battle
            Battle.BattleManager.Instance.HandleRequest(pack);
        }
    }
}
