public class TServerDataLogin
{
    /**
     * 玩家信息
     */
    public string userInfo { get; set; }
    
    public long power { get; set; }
    
    /**
     * 用户id
     */
    public string gameUid { get; set; }
    
    /**
     * 服务器0点时间戳
     */
    public long systemZeroTime { get; set; }
    
    /**
     * 注册时间戳
     */
    public long regTime { get; set; }
    
    /**
     * 服务器时间
     */
    public long serverTime { get; set; }
    
    /**
     * 玩家设备id
     */
    public string deviceId { get; set; }
    
    /**
     * 登录是否是注册
     */
    public bool isRegister { get; set; }
    
    /**
     * 每日签到
     */
    public object signIn { get; set; }
    
    /**
     * 日常周常
     */
    public object residentQuest { get; set; }
    
    /**
     * 成就
     */
    public object[] achieves { get; set; }
    
    public object[] items { get; set; }
}

public class TMapData
{
 public int[,] MapRoom;
}