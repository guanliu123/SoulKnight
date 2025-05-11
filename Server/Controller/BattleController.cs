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

                // 创建用于最终返回给请求者的包（通常是确认成功或失败）
                MainPack returnPack = new MainPack();
                returnPack.RequestCode = RequestCode.Battle;
                returnPack.ActionCode = ActionCode.StartEnterBattle; // 保持 ActionCode 一致

                // 检查房间是否存在
                if (string.IsNullOrEmpty(roomName) || !RoomManager.Instance.RoomExists(roomName)) // 增加存在性检查
                {
                    Console.WriteLine($"StartEnterBattle: 房间 '{roomName}' 不存在或名称为空。");
                    returnPack.ReturnCode = ReturnCode.Fail;
                    return returnPack;
                }

                // 从 RoomManager 获取房间内的所有玩家
                Room room = RoomManager.Instance.GetRoom(roomName); // 获取 Room 对象
                if (room == null) // 再次检查 Room 对象
                {
                    Console.WriteLine($"StartEnterBattle: 获取房间 '{roomName}' 失败。");
                    returnPack.ReturnCode = ReturnCode.Fail;
                    return returnPack;
                }
                List<Client> players = room.Clients; // 直接从 Room 获取客户端列表

                // 如果是房间内第一个请求进入战斗的玩家，创建战斗并发送初始化信息
                if (!roomToBattleId.ContainsKey(roomName))
                {
                    Console.WriteLine($"StartEnterBattle: 房间 '{roomName}' 尚未创建战斗，开始创建...");
                    // 准备战斗玩家信息
                    List<BattlePlayerPack> battlePlayers = new List<BattlePlayerPack>();
                    int battlePlayerId = 1;
                    foreach (Client playerClient in players)
                    {
                        if (playerClient != null)
                        {
                            BattlePlayerPack playerPack = new BattlePlayerPack();
                            playerPack.Id = playerClient.Id; // 客户端唯一ID
                            playerPack.Battleid = battlePlayerId++; // 战斗内ID
                            playerPack.Playername = playerClient.userName;
                            battlePlayers.Add(playerPack);
                        }
                        else
                        {
                            Console.WriteLine($"StartEnterBattle: 玩家客户端为空，跳过...");
                        }
                    }

                    // --- 移动随机种子生成到这里 ---
                    Random random = new Random();
                    int seedValue = random.Next(0, 10000);
                    Console.WriteLine($"StartEnterBattle: 为房间 '{roomName}' 生成随机种子: {seedValue} {players.Count}");

                    // 创建战斗
                    int battleId = BattleManager.Instance.BeginBattle(battlePlayers, seedValue);
                    roomToBattleId[roomName] = battleId; // 记录房间与战斗的关联



                    pack.CharacterPacks.Clear();
                    foreach (Client c in room.Clients)
                    {
                        if (c.PlayerType != null && c.PlayerType.Length != 0)
                        {
                            CharacterPack p = new CharacterPack();
                            p.CharacterName = c.userName;
                            p.PlayerType = c.PlayerType;
                            pack.CharacterPacks.Add(p);
                        }
                    }
                    BattleInitInfo battleInitInfo = new BattleInitInfo();
                    battleInitInfo.RandSeed = seedValue;
                    pack.BattleInitInfo = battleInitInfo;
                    returnPack.BattleInitInfo = battleInitInfo;
                    pack.IsBroadcastMessage = true;
                    room.Broadcast(client, pack);
                    pack.IsBroadcastMessage = false;
                    Console.WriteLine($"StartEnterBattle: 向房间 '{roomName}' 的所有客户端发送 BattleInitInfo (TCP)...");
                }
                else
                {
                    Console.WriteLine($"StartEnterBattle: 房间 '{roomName}' 的战斗已存在 (ID: {roomToBattleId[roomName]})。");
                    // 如果战斗已存在，可能需要向后加入的玩家发送不同的信息或直接返回成功
                }
                returnPack.ReturnCode = ReturnCode.Success;
                return returnPack;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartEnterBattle 发生严重错误: {ex}");
                MainPack returnPack = new MainPack();
                returnPack.RequestCode = RequestCode.Battle;
                returnPack.ActionCode = ActionCode.StartEnterBattle;
                returnPack.ReturnCode = ReturnCode.Fail;
                return returnPack;
            }
        }


    }
}