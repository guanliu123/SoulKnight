using SoulKnightProtocol;
using Battle;
//无用代码
namespace KnightServer
{
    public class BattleController : BaseController
    {
        // 房间名称到战斗ID的映射
        private Dictionary<string, int> roomToBattleId = new Dictionary<string, int>();

        public BattleController(ControllerManager manager) : base(manager)
        {
            requestCode = RequestCode.Battle;
        }

        // 处理玩家操作请求
        public MainPack BattlePushDowmPlayerOpeartions(Client client, MainPack pack)
        {
            // 处理玩家操作逻辑
            // 调用战斗内部接口，记录操作，然后转发
            string roomName = "";
            if (pack.RoomPacks.Count > 0)
            {
                roomName = pack.RoomPacks[0].RoomName;
            }
            Room room = RoomManager.Instance.GetRoom(roomName);
            if (room != null)
            {
                pack.IsBroadcastMessage = true;
                pack.ReturnCode = ReturnCode.Success;
                room.BroadcastTo(null, pack);
                pack.IsBroadcastMessage = false;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }

        // 处理游戏结束请求
        public MainPack ClientSendGameOver(Client client, MainPack pack)
        {
            return null;
        }

        // 处理玩家进入战斗请求
        public MainPack StartEnterBattle(Client client, MainPack pack)
        {
            try
            {
                string roomName = "";
                if (pack.RoomPacks.Count > 0)
                {
                    roomName = pack.RoomPacks[0].RoomName;
                }


                // 创建返回包
                MainPack returnPack = new MainPack();
                returnPack.RequestCode = RequestCode.Battle;
                returnPack.ActionCode = ActionCode.StartEnterBattle;

                // 检查房间是否存在
                if (string.IsNullOrEmpty(roomName))
                {
                    returnPack.ReturnCode = ReturnCode.Fail;
                    return returnPack;
                }

                // 从 RoomManager 获取房间内的所有玩家
                List<Client> players = RoomManager.Instance.GetRoomPlayers(roomName);
                returnPack.ReturnCode = ReturnCode.Success;
                // 如果是房间内第一个请求进入战斗的玩家，创建战斗
                if (!roomToBattleId.ContainsKey(roomName))
                {
                    // 准备战斗玩家信息
                    Room room = RoomManager.Instance.GetRoom(roomName);
                    List<BattlePlayerPack> battlePlayers = new List<BattlePlayerPack>();
                    // 为每个玩家创建战斗信息
                    int battlePlayerId = 1;
                    foreach (Client playerClient in players)
                    {
                        if (playerClient != null)
                        {
                            BattlePlayerPack playerPack = new BattlePlayerPack();
                            playerPack.Id = playerClient.Id;
                            playerPack.Battleid = battlePlayerId++;
                            playerPack.Playername = playerClient.userName;
                            battlePlayers.Add(playerPack);
                        }
                    }

                    // 创建战斗
                    int battleId = BattleManager.Instance.BeginBattle(battlePlayers);
                    room.BroadcastTo(client, pack);
                    // 记录房间与战斗的关联
                    roomToBattleId[roomName] = battleId;

                    Console.WriteLine($"为房间 {roomName} 创建了战斗 {battleId}");
                }

                return returnPack;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartEnterBattle 错误: {ex.Message}");
                MainPack returnPack = new MainPack();
                returnPack.RequestCode = RequestCode.Battle;
                returnPack.ActionCode = ActionCode.StartEnterBattle;
                returnPack.ReturnCode = ReturnCode.Fail;
                return returnPack;
            }
        }


    }
}