
#region

using System.Collections.Generic;

#endregion

public class MsgIdSettings
{
    public const int MSG_QUEUE_MAX = 50;
    public static MsgIdSetting MsgIdSettingDefault = new(MSG_QUEUE_MAX);
    
    public enum MsgIdSettingsVerifyResult
    {
        /**
         * 禁止发送
         */
        None,
        
        /**
         * 仅发送
         */
        Send,
        
        /**
         * 发送并存入历史纪录
         */
        SendAndSave
    };
    
    public static Dictionary<RequestId, MsgIdSetting> dict = new()
    {
        /** HEART (非阻塞心跳消息，不参与断线重发) */
        { RequestId.NO_QUEUE_HEART, new MsgIdSetting(0) },
        /** LOGIN (登录消息，不参与断线重发) */
        { RequestId.LOGIN, new MsgIdSetting(0) },
        /** 登录成功的消息(只保留最近一条) */
        { RequestId.LOGIN_OVER, new MsgIdSetting(1, true) }
    };
    
    public static MsgIdSetting GetSetting(RequestId id)
    {
        MsgIdSetting ret = MsgIdSettingDefault;
        MsgIdSetting exist = dict[id];
        if (exist != null)
        {
            ret = exist;
        }
        
        return ret;
    }
    
    //public static MsgIdSettingsVerifyResult VerifyIdBySetting(RequestId id, List<ISocketPackRequest> cachePacks)
    //{
    //	MsgIdSetting setting = GetSetting(id);
    //	if (setting != null) {
    //		int counter = 0;
    //		int firstIndex = -1;
    //		for (int i = cachePacks.Count - 1; i >= 0; i--) {
    //			ISocketPackRequest pack = cachePacks[i];
    //			if (pack.i)
    //		}
    //	}
    //}
}

public class MsgIdSetting
{
    /**
     * 限制同一ID允许在消息队列中存在的数量:
     * - 小于或者等于0时，消息不出现在消息队列中 (例如LOGIN)
     * - 等于1时，表示消息只能同时有一个在消息队列中
     * - 大于1时，表示消息同时能有n个在消息队列中
     */
    private int? limitCount;
    
    /** 重复消息时是否只保留最后发送的消息
     * - 为true时，消息队列只保留最后发送的消息
     * - 为false时，消息队列只保留最早发送的消息
     */
    private bool? keepLast;
    
    public MsgIdSetting(int? _limitCount, bool? _keepLast = null)
    {
        if (_limitCount != null)
        {
            limitCount = _limitCount;
        }
        
        if (_keepLast != null)
        {
            keepLast = _keepLast;
        }
    }
}