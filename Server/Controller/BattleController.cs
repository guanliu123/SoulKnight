using SoulKnightProtocol;
using Battle;

namespace KnightServer
{
    public class BattleController : BaseController
    {
        public BattleController(ControllerManager manager) : base(manager)
        {
            requestCode = RequestCode.Battle;
        }
        
        // 处理战斗开始请求
        public MainPack BattleStart(Client client, MainPack pack)
        {
            // 处理战斗开始逻辑
            return null;
        }
        
        // 处理玩家操作请求
        public MainPack BattlePushDowmPlayerOpeartions(Client client, MainPack pack)
        {
            // 处理玩家操作逻辑
            return null;
        }
        
        // 处理游戏结束请求
        public MainPack ClientSendGameOver(Client client, MainPack pack)
        {
            // 处理游戏结束逻辑
            return null;
        }
        public MainPack BattleReview(Client client, MainPack pack){
            return null;
        }
        public MainPack BattlePushDowmAllFrameOpeartions(Client client, MainPack pack){
            return null;
        }
        public MainPack BattlePusBattleReadyhDowmAllFrameOpeartions(Client client, MainPack pack){
            return null;
        }
        public MainPack StartEnterBattle(Client client, MainPack pack){
            return null;
        }
    
        
    }
}