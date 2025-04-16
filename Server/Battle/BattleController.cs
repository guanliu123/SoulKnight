using SoulKnightProtocol;
using System;
using System.Collections.Generic;
using System.Threading;
using KnightServer; // 使用正确的命名空间

namespace Battle
{
    /// <summary>
    /// 战斗控制器
    /// 负责单场战斗的逻辑处理和帧同步
    /// </summary>
    public class BattleController
    {
        // 战斗ID
        public int battleID { get; private set; }
        
        // 服务器引用
        private KnightServer.Server server; // 使用完整的类型名称
        
        // 玩家信息
        private Dictionary<int, int> dic_battleUserUid; // <玩家ID, 战斗内ID>
        private Dictionary<int, string> dic_battleid_ip_port; // <战斗内ID, IP:端口>
        private int playerCount; // 存储玩家数量
        
        // 准备阶段
        private Dictionary<int, bool> dic_battleReady; // <战斗内ID, 准备状态>
        private bool isBeginBattle = false;
        
        // 战斗阶段
        private int frameid; // 服务器上每个比赛对象，都有一个成员的freamid，保存了当前比赛下一帧要进入的id
        private Dictionary<int, AllPlayerOperation> dic_match_frames; // 用来保存我们所有玩家的每帧的操作
        private Dictionary<int, InputPack> dic_next_frame_opts; // 每帧服务器将采集来的客户端的操作都存放在这里
        private Dictionary<int, int> dic_next_opts_frameid; // 玩家的帧id
        private static object lockThis = new object(); // 锁
        private bool _isRun = false;  // isRun判断是否需要一直传帧
        
        // 结束阶段
        private Timer waitBattleFinish;
        private float finishTime; // 结束倒计时
        private Dictionary<int, bool> dic_playerGameOver; // 记录玩家游戏结束
        private bool oneGameOver; // 有人结束了
        private bool allGameOver; // 全部人都结束了
        
        // 帧率设置
        private const int FRAME_RATE = 15; // 15FPS
        private const int FRAME_INTERVAL = 1000 / FRAME_RATE; // 66.67ms

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="server">服务器实例</param>
        /// <param name="_battleID">战斗ID</param>
        /// <param name="_battleUser">参战玩家</param>
        public BattleController(KnightServer.Server server, int battleID, List<BattlePlayerPack> battleUsers)
        {
            this.server = server;
            this.battleID = battleID;
            
            // 初始化字段，避免 null 引用
            dic_battleUserUid = new Dictionary<int, int>();
            dic_battleid_ip_port = new Dictionary<int, string>();
            dic_battleReady = new Dictionary<int, bool>();
            dic_match_frames = new Dictionary<int, AllPlayerOperation>();
            dic_next_frame_opts = new Dictionary<int, InputPack>();
            dic_next_opts_frameid = new Dictionary<int, int>();
            dic_playerGameOver = new Dictionary<int, bool>();
            waitBattleFinish = null!; // 使用 null! 表示稍后初始化
            
            // 注册UDP消息处理
            UdpManager.Instance.RegisterHandler(Handle);
            
            // 初始化战斗信息
            ThreadPool.QueueUserWorkItem((obj) => {
                dic_battleUserUid = new Dictionary<int, int>();
                dic_battleid_ip_port = new Dictionary<int, string>();
                dic_battleReady = new Dictionary<int, bool>();
                
                int userBattleID = 0;
                
                // 准备战斗信息包
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Room;
                pack.ReturnCode = ReturnCode.Success;
                pack.ActionCode = ActionCode.StartEnterBattle;
                
                BattleInfo battleInfo = new BattleInfo();
                
                // 分配战斗内ID并准备玩家信息
                playerCount = battleUsers.Count;
                for (int i = 0; i < playerCount; i++)
                {
                    int _userUid = battleUsers[i].Id; // 修改为 Id 而不是 uid
                    userBattleID++;  // 为每个user设置一个battleID，从1开始
                    
                    dic_battleUserUid[_userUid] = userBattleID;
                    dic_battleReady[userBattleID] = false;
                    
                    // 创建玩家信息包
                    BattlePlayerPack playerPack = new BattlePlayerPack();
                    playerPack.Id = _userUid;
                    playerPack.Battleid = userBattleID;
                    playerPack.Playername = battleUsers[i].Playername; // 修改为 Playername 而不是 userName
                    playerPack.Hero = battleUsers[i].Hero; // 修改为 Hero 而不是 hero
                    playerPack.Teamid = battleUsers[i].Teamid; 
                    
                    battleInfo.BattleUserInfo.Add(playerPack);
                }
                
                pack.BattleInfo = battleInfo;
                
                // 向所有玩家发送战斗信息
                Console.WriteLine($"向客户端发送战场数据！{pack}");
                for (int i = 0; i < battleUsers.Count; i++)
                {
                    int _userUid = battleUsers[i].Id; // 修改为 Id 而不是 uid
                    Client client = server.GetClientByID(_userUid);
                    if (client != null)
                    {
                        client.Send(pack);
                    }
                }
                
                isBeginBattle = false;
            }, null);
        }

        /// <summary>
        /// 处理UDP消息
        /// </summary>
        public void Handle(MainPack pack)
        {
            if (pack == null) return;
            
            switch (pack.ActionCode)
            {
                case ActionCode.BattleReady:
                    // 玩家发送"我准备好了"
                    if (isBeginBattle) return;
                    
                    dic_battleReady[pack.BattlePlayerPack[0].Battleid] = true;
                    dic_battleid_ip_port[pack.BattlePlayerPack[0].Battleid] = pack.Str;
                    
                    // 检查是否所有玩家都已准备
                    isBeginBattle = true;
                    foreach (var item in dic_battleReady.Values)
                    {
                        isBeginBattle = (isBeginBattle && item);
                    }
                    
                    if (isBeginBattle)
                    {
                        // 开始战斗
                        BeginBattle();
                    }
                    break;
                    
                case ActionCode.BattlePushDowmPlayerOpeartions:
                    // 玩家发送"玩家认为的"帧消息
                    if (!isBeginBattle) return;
                    
                    BattleInfo battleInfo = pack.BattleInfo;
                    int syncFrameId = battleInfo.OperationID; // 玩家操作帧
                    InputPack operation = battleInfo.SelfOperation; // 玩家操作，使用 InputPack
                    
                    UpdatePlayerOperation(operation, syncFrameId);
                    break;
                    
                case ActionCode.ClientSendGameOver:
                    // 玩家游戏结束
                    UpdatePlayerGameOver(int.Parse(pack.Str));
                    break;
            }
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        private void BeginBattle()
        {
            frameid = 1;
            _isRun = true;
            oneGameOver = false;
            allGameOver = false;
            
            // 生成随机种子
            Random random = new Random();
            int randSeed = random.Next(0, 10000);
            
            // 初始化帧操作数据
            dic_match_frames = new Dictionary<int, AllPlayerOperation>();
            dic_next_frame_opts = new Dictionary<int, InputPack>();
            dic_next_opts_frameid = new Dictionary<int, int>();
            dic_playerGameOver = new Dictionary<int, bool>();
            
            // 初始化玩家状态
            foreach (int id in dic_battleUserUid.Values)
            {
                dic_next_frame_opts[id] = null;
                dic_next_opts_frameid[id] = 0;
                dic_playerGameOver[id] = false;
            }
            
            // 启动帧同步线程
            Thread frameThread = new Thread(Thread_SendFrameData);
            frameThread.Start();
            
            Console.WriteLine("开始战斗");
            
            // 向所有玩家发送战斗开始消息，包含随机种子
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Battle;
            pack.ActionCode = ActionCode.BattleStart;
            
            // 使用 BattleInitInfo 而不是 BattleInfo
            BattleInitInfo battleInitInfo = new BattleInitInfo();
            battleInitInfo.RandSeed = randSeed;
            pack.BattleInitInfo = battleInitInfo;
            
            foreach (var item in dic_battleid_ip_port)
            {
                UdpManager.Instance.Send(pack, item.Value);
            }
        }

        /// <summary>
        /// 帧同步线程
        /// </summary>
        private void Thread_SendFrameData()
        {
            Console.WriteLine("开始战斗");
            
            bool isFinishBS = false;
            
            // 循环检查是否所有玩家都发送了第一帧操作
            while (!isFinishBS)
            {
                // 检查是否所有玩家都发送了第一帧操作
                bool allData = true;
                foreach (var op in dic_next_frame_opts.Values)
                {
                    if (op == null)
                    {
                        allData = false;
                        break;
                    }
                }
                
                if (allData)
                {
                    Console.WriteLine("战斗服务器:收到全部玩家的第一次操作数据");
                    frameid = 1;
                    isFinishBS = true;
                }
                
                Thread.Sleep(500);
            }
            
            Console.WriteLine("开始发送帧数据");
            
            // 主帧同步循环
            while (_isRun)
            {
                if (oneGameOver)
                {
                    break; // 有玩家游戏结束，停止帧同步
                }
                else
                {
                    // 收集当前帧的所有玩家操作
                    AllPlayerOperation nextFrameOpt = new AllPlayerOperation();
                    
                    try
                    {
                        for (int i = 1; i <= playerCount; i++)
                        {
                            if (dic_next_frame_opts.ContainsKey(i))
                            {
                                nextFrameOpt.Operations.Add(dic_next_frame_opts[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        nextFrameOpt = new AllPlayerOperation();
                    }
                    
                    // 保存当前帧操作
                    nextFrameOpt.Frameid = frameid;
                    dic_match_frames.Add(frameid, nextFrameOpt);
                    
                    // 向所有玩家发送帧操作
                    foreach (var item in dic_battleid_ip_port)
                    {
                        // 如果没游戏结束才发送
                        if (!dic_playerGameOver[item.Key])
                        {
                            SendUnsyncFrames(item.Value, item.Key);
                        }
                    }
                    
                    // 进入下一帧
                    frameid++;
                    
                    // 清空下一帧操作缓存
                    lock (lockThis)
                    {
                        dic_next_frame_opts.Clear();
                    }
                }
                
                // 按帧率休眠
                Thread.Sleep(FRAME_INTERVAL);
            }
            
            Console.WriteLine("帧数据发送线程结束.....................");
        }

        /// <summary>
        /// 发送服务器认为玩家还没同步的帧
        /// </summary>
        /// <param name="ipPort">IP:端口</param>
        /// <param name="battleId">战斗内玩家ID</param>
        private void SendUnsyncFrames(string ipPort, int battleId)
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Battle;
            pack.ActionCode = ActionCode.BattlePushDowmAllFrameOpeartions;
            
            BattleInfo battleInfo = new BattleInfo();
            
            // 发送玩家尚未同步的帧
            for (int i = dic_next_opts_frameid[battleId] + 1; i <= frameid; i++)
            {
                battleInfo.AllPlayerOperation.Add(dic_match_frames[i]);
            }
            
            battleInfo.OperationID = frameid;
            pack.BattleInfo = battleInfo;
            
            UdpManager.Instance.Send(pack, ipPort);
        }

        /// <summary>
        /// 更新玩家操作
        /// </summary>
        /// <param name="operation">玩家操作</param>
        /// <param name="syncFrameId">同步帧ID</param>
        private void UpdatePlayerOperation(InputPack operation, int syncFrameId)
        {
            // 更新玩家已同步帧ID
            if (dic_next_opts_frameid[operation.BattleId] < syncFrameId - 1)
            {
                dic_next_opts_frameid[operation.BattleId] = syncFrameId - 1;
            }
            
            // 丢弃过时的操作
            if (syncFrameId != frameid)
            {
                return;
            }
            
            // 保存玩家操作
            lock (lockThis)
            {
                dic_next_frame_opts[operation.BattleId] = operation;
            }
        }

        /// <summary>
        /// 处理玩家游戏结束
        /// </summary>
        /// <param name="battleId">战斗内玩家ID</param>
        private void UpdatePlayerGameOver(int battleId)
        {
            oneGameOver = true;
            dic_playerGameOver[battleId] = true;
            
            // 检查是否所有玩家都已结束
            allGameOver = true;
            foreach (bool playerGameOver in dic_playerGameOver.Values)
            {
                if (!playerGameOver)
                {
                    allGameOver = false;
                    break;
                }
            }
            
            if (allGameOver)
            {
                if (waitBattleFinish != null) return;
                
                Console.WriteLine("战斗即将结束咯......");
                _isRun = false;
                
                finishTime = 3000f; // 3秒倒计时
                waitBattleFinish = new Timer(EndBattleCountdown, null, 1000, 1000);
            }
        }

        /// <summary>
        /// 战斗结束倒计时
        /// </summary>
        private void EndBattleCountdown(object? state)
        {
            finishTime -= 1000f;
            Console.WriteLine($"战斗结束倒计时: {finishTime/1000}秒");
            
            if (finishTime <= 0)
            {
                // 向所有玩家发送游戏结束消息
                foreach (var item in dic_battleid_ip_port)
                {
                    SendGameOverMessage(item.Value);
                }
                
                // 停止计时器
                waitBattleFinish?.Dispose();
                waitBattleFinish = null;
                
                // 通知战斗管理器结束战斗
                Battle.BattleManager.Instance.FinishBattle(battleID, dic_match_frames);
            }
        }

        /// <summary>
        /// 发送游戏结束消息
        /// </summary>
        /// <param name="ipPort">IP:端口</param>
        private void SendGameOverMessage(string ipPort)
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Battle;
            pack.ActionCode = ActionCode.BattlePushDowmGameOver;
            pack.Str = "1";
            
            UdpManager.Instance.Send(pack, ipPort);
        }
    }
}