using SoulKnightProtocol;
using System;
using System.Collections.Generic;
using System.Threading;
using KnightServer; // 使用正确的命名空间

namespace Battle
{
    /// <summary>
    /// 战斗管理器 - 单例模式
    /// 负责创建、管理和结束战斗
    /// </summary>
    public class BattleManager
    {
        private static BattleManager instance;
        public static BattleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BattleManager();
                }
                return instance;
            }
        }

        // 战斗ID计数器
        private int battleID;
        // 战斗控制器字典 <战斗ID, 战斗控制器>
        private Dictionary<int, BattleController> dic_battles;
        // 战斗玩家信息字典 <战斗ID, 玩家列表>
        private Dictionary<int, List<BattlePlayerPack>> dic_battleUserInfo;
        // 服务器引用
        private KnightServer.Server server; // 使用完整的类型名称
        
        // 战斗相关字典
        public Dictionary<int, int> playerToBattleId { get; private set; } // <玩家ID, 战斗ID>
        private Dictionary<int, List<int>> battleToPlayers; // <战斗ID, 玩家ID列表>

        private BattleManager()
        {
            battleID = 0;
            dic_battles = new Dictionary<int, BattleController>();
            dic_battleUserInfo = new Dictionary<int, List<BattlePlayerPack>>();
            playerToBattleId = new Dictionary<int, int>();
            battleToPlayers = new Dictionary<int, List<int>>();
            server = null; // 初始化为 null，在 Initialize 方法中赋值
        }
        
        /// <summary>
        /// 初始化战斗管理器
        /// </summary>
        /// <param name="server">服务器实例</param>
        public void Initialize(KnightServer.Server server) // 使用完整的类型名称
        {
            this.server = server;
        }

        /// <summary>
        /// 将玩家添加到战斗
        /// </summary>
        public void AddPlayerToBattle(int playerId, int battleId)
        {
            playerToBattleId[playerId] = battleId;
            
            if (!battleToPlayers.ContainsKey(battleId))
            {
                battleToPlayers[battleId] = new List<int>();
            }
            
            battleToPlayers[battleId].Add(playerId);
        }
        
        /// <summary>
        /// 从战斗中移除玩家
        /// </summary>
        public void RemovePlayerFromBattle(int playerId)
        {
            if (playerToBattleId.ContainsKey(playerId))
            {
                int battleId = playerToBattleId[playerId];
                
                if (battleToPlayers.ContainsKey(battleId))
                {
                    battleToPlayers[battleId].Remove(playerId);
                }
                
                playerToBattleId.Remove(playerId);
            }
        }
        
        /// <summary>
        /// 获取战斗中的所有玩家
        /// </summary>
        public List<int> GetBattlePlayers(int battleId)
        {
            if (battleToPlayers.ContainsKey(battleId))
            {
                return battleToPlayers[battleId];
            }
            
            return new List<int>();
        }
        
        /// <summary>
        /// 清理战斗数据
        /// </summary>
        public void ClearBattleData(int battleId)
        {
            if (battleToPlayers.ContainsKey(battleId))
            {
                battleToPlayers.Remove(battleId);
            }
        }
        private int GenerateBattleId()
        {
            return Interlocked.Increment(ref battleID);
        }

        /// <summary>
        /// 开始一场新战斗
        /// </summary>
        /// <param name="_battleUser">参战玩家列表</param>
        // 修改 BeginBattle 签名以接受 seedValue
        public int BeginBattle(List<BattlePlayerPack> battleUsers, int seedValue)
        {
            int battleId = GenerateBattleId(); // 生成唯一的战斗 ID
            // 将 server 实例和 seedValue 传递给 BattleController 构造函数
            BattleController newBattle = new BattleController(this.server, battleId, battleUsers, seedValue);
            dic_battles.TryAdd(battleId, newBattle); // 使用 TryAdd 保证线程安全
            Console.WriteLine($"BattleManager: 创建了新的战斗，ID: {battleId}, Seed: {seedValue}");
            return battleId;
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        /// <param name="_battleID">战斗ID</param>
        /// <param name="dic_match_frames">所有帧操作记录</param>
        public void FinishBattle(int _battleID, Dictionary<int, AllPlayerOperation> dic_match_frames)
        {
            dic_battles.Remove(_battleID);
            
            // 准备战斗回放数据
            MainPack mainPack = new MainPack();
            mainPack.ActionCode = ActionCode.BattleReview;
            
            BattleInfo battleInfo = new BattleInfo();
            
            // 添加玩家信息
            int userBattleID = 0;
            int playerCount = dic_battleUserInfo[_battleID].Count;
            for (int i = 0; i < playerCount; i++)
            {
                int _userUid = dic_battleUserInfo[_battleID][i].Id;
                userBattleID++;  // 为每个user设置一个battleID，从1开始
                
                BattlePlayerPack _bUser = new BattlePlayerPack();
                _bUser.Id = _userUid;
                _bUser.Battleid = userBattleID;
                _bUser.Playername = dic_battleUserInfo[_battleID][i].Playername;
                _bUser.Hero = dic_battleUserInfo[_battleID][i].Hero;
                _bUser.Teamid = dic_battleUserInfo[_battleID][i].Teamid;
                
                battleInfo.BattleUserInfo.Add(_bUser);
            }
            
            // 添加所有帧操作
            foreach (AllPlayerOperation allPlayerOperation in dic_match_frames.Values)
            {
                battleInfo.AllPlayerOperation.Add(allPlayerOperation);
            }
            
            mainPack.BattleInfo = battleInfo;
            
            // 向所有参与战斗的玩家发送战斗回放数据
            foreach (int uid in GetBattlePlayers(_battleID))
            {
                RemovePlayerFromBattle(uid);
                Client client = server.GetClientByID(uid);
                if (client != null)
                {
                    client.Send(mainPack);
                }
            }
            
            // 清理战斗相关数据
            ClearBattleData(_battleID);
            
            Console.WriteLine($"战斗结束。。。。。BattleID：{_battleID}");
        }

        // 从消息包中获取战斗ID
        private int GetBattleIdFromPack(MainPack pack)
        {
            // 根据消息包的内容获取战斗ID
            if (pack.ActionCode == ActionCode.BattleReady && pack.BattlePlayerPack.Count > 0)
            {
                // 从战斗玩家包中获取玩家ID
                int playerId = pack.BattlePlayerPack[0].Id;
                
                // 通过玩家ID查找战斗ID
                if (playerToBattleId.ContainsKey(playerId))
                {
                    return playerToBattleId[playerId];
                }
            }
            else if (pack.ActionCode == ActionCode.BattlePushDowmPlayerOpeartions && pack.BattleInfo != null)
            {
                // 从战斗信息中获取玩家ID
                if (pack.BattleInfo.SelfOperation != null)
                {
                    int battleId = pack.BattleInfo.SelfOperation.BattleId;
                    return battleId;
                }
            }
            else if (pack.ActionCode == ActionCode.ClientSendGameOver)
            {
                // 从字符串中获取战斗ID
                if (int.TryParse(pack.Str, out int battleId))
                {
                    return battleId;
                }
            }
            
            // 如果无法确定战斗ID，返回默认值
            return -1;
        }
        
        // 添加 HandleRequest 方法
        public void HandleRequest(MainPack pack)
        {
            if (pack == null) return;
            
            // 获取战斗ID
            int battleId = GetBattleIdFromPack(pack);
            
            if (battleId > 0 && dic_battles.TryGetValue(battleId, out BattleController battle))
            {
                battle.Handle(pack);
            }
            else
            {
                Console.WriteLine($"未找到对应的战斗控制器，战斗ID: {battleId}");
            }
        }
        // 删除这里多余的大括号和重复的方法实现
    }
}