
#region

using Newtonsoft.Json.Linq;

#endregion

public class LoginResponse : ResponseBase
{
    /**
     * 登录时服务器下发的状态值定义
     */
    public enum LoginResponseStatus
    {
        LOGIN_OK = 0,
        
        //------------------------------ 需要提示并退出的情况
        /**
         * 停服维护
         */
        SERVER_MAINTAIN = 100,
        
        /**
         * 封号
         */
        USER_BANNED = 103,
        
        /**
         * 封设备
         */
        DEVICE_BANNED = 104,
        
        /**
         * 停服时踢下线
         */
        SHUTDOWN_KICK = 111,
        
        /**
         * GM操作踢下线
         */
        GM_KICK = 113,
        
        /**
         * 客户端版本号格式非法
         */
        CLIENT_VERSION_ERROR = 126,
        
        /**
         * 模拟器登录自动拦截
         */
        SIMULATOR_LOGIN_AUTO_FAIL = 127,
        
        /**
         * 玩家不属于这个服务器
         */
        USER_NOT_BELONG_SERVER = 128,
        
        /**
         * IDC IP 拒绝登录
         */
        IDC_IP_NOT_LOGIN = 129,
        
        /**
         * 版本太旧
         */
        VERSION_TOO_OLD = 131,
        
        //------------------------------ 需要重试的情况
        /**
         * 登录请求信息错误
         */
        INVALID_LOGIN_INFO = 101,
        
        /**
         * 用户未登录，用于发命令时找不到用户的情况
         */
        USER_NOT_LOGIN = 108,
        
        /**
         * 通用的登录失败，可能用于各自原因，一般是服务器端报错引起
         */
        LOGIN_FAIL = 109,
        
        /**
         * 超时被踢下线
         */
        IDLE_KICK = 112,
        
        /**
         * 其它断线情况
         */
        UNKNOW_KICK = 114,
        
        /**
         * 一个号同时登录，后登录的会把之前登录的踢下线
         */
        LOGIN_KICK = 110,
        
        //------------------------------ 不需要提示的情况
        /**
         * 同一个session连接，重复发送登录请求时
         */
        USER_ALREADY_LOGIN = 121,
        
        /**
         * 目前未使用
         */
        TOKEN_TIME_OVERDUE = 116
    }
    
    private bool firstLogined;
    
    protected override void Handler(ISocketPack pack)
    {
        base.Handler(pack);
        if (!onLoginFail(pack))
        {
            LogTool.Log("Login Server failed");
            return;
        }
        
        if ((LoginResponseStatus)pack.s == LoginResponseStatus.LOGIN_OK)
        {
            LogTool.Log("Login Server OK");
            JObject _obj = JObject.Parse((string)pack.d);
            LogTool.Log("Login Server params : " + pack.d);
            TServerDataLogin data = new();
            GameTool.ConvertJObjectToClass(data, _obj);
            TimeCenter.ServerTime = data.serverTime;
            LoginDeal(data);
            GlobalData.Instance.JumpServer = false;
            GlobalData.Instance.changeServerId = null;
            GlobalData.Instance.changeServerIp = null;
        }
    }
    
    private bool onLoginFail(ISocketPack pack)
    {
        /** 处理方案： 0=重试，1=退出 */
        //int handleMode = 0;
        //string promptContent = "";
        //string promptTitle = "";//LOCAL.getText("title_note");
        if (pack != null)
        {
            int status = (int)pack.s;
            JObject obj = JObject.Parse((string)pack.d ?? string.Empty);
            if (status != 0)
            {
                NetManager.Instance.SetAutoReconnectOnClose(false);
                NetManager.Instance.Dispose();
            }
            else
            {
                string uid = (string)obj["gameUid"];
                NetManager.Instance.checkHistoryPacksAfterLogin(uid);
            }
            
            switch ((LoginResponseStatus)status)
            {
                case LoginResponseStatus.USER_ALREADY_LOGIN:
                case LoginResponseStatus.TOKEN_TIME_OVERDUE:
                case LoginResponseStatus.USER_NOT_BELONG_SERVER:
                case LoginResponseStatus.INVALID_LOGIN_INFO:
                {
                    if ((LoginResponseStatus)status == LoginResponseStatus.USER_NOT_BELONG_SERVER)
                    {
                        //GLOBAL.JumpServer = false;
                        //GLOBAL.changeServerId = GLOBAL.changeServerIp = null;
                    }
                    
                    //ResetGame.doResetGame(false);
                    return true;
                }
            }
            
            if ((LoginResponseStatus)status == LoginResponseStatus.LOGIN_OK && obj["data"] != null)
            {
                //登录成功
                return false;
            }
            
            if ((LoginResponseStatus)status != LoginResponseStatus.LOGIN_OK)
            {
                EventManager.Instance.Emit(EventId.SET_LOADING_TEXT, status);
            }
            
            switch ((LoginResponseStatus)status)
            {
                //------------------------------ 需要提示并退出的情况
                case LoginResponseStatus.USER_BANNED:
                case LoginResponseStatus.DEVICE_BANNED:
                    //promptContent = LOCAL.getText("error_tips_acc_banned"); //pubilc901  此帐号已被暂停访问
                    //handleMode = 1;
                    break;
                
                case LoginResponseStatus.GM_KICK:
                    //promptContent = LOCAL.getText("error_tips_connect_poor"); //pubilc902    与服务器连接断开，请稍后重试
                    //handleMode = 1;
                    break;
                
                case LoginResponseStatus.SERVER_MAINTAIN:
                    //promptContent = LOCAL.getText("error_tips_maintenance"); //pubilc903     服务器维护中
                    //handleMode = 1;
                    break;
                case LoginResponseStatus.VERSION_TOO_OLD:
                    //promptContent = LOCAL.getText("error_tips_out_date"); //system_notice001  您的游戏版本过旧，请更新到最新版本尝试，如有问题请联系客服人员。
                    //handleMode = 1;
                    //  = "";
                    break;
                case LoginResponseStatus.IDC_IP_NOT_LOGIN:
                case LoginResponseStatus.SIMULATOR_LOGIN_AUTO_FAIL:
                    //promptContent = LOCAL.getText("error_tips_connect_failed");
                    //handleMode = 1;
                    break;
            }
        }
        else
        {
            LogTool.LogError("(onLoginFail) 整个数据包为空, 消息头都没有");
        }
        
        return true;
    }
    
    private void LoginDeal(TServerDataLogin data)
    {
        string text = JsonTool.SerializeObject(data);
        LogTool.Log("(Login) LoginDeal " + text);
        //todo:更新玩家信息
        //UserManager.Instance.UpdateData(data);
        bool isRegister = false;
        if (!firstLogined)
        {
            firstLogined = true;
            isRegister = data.isRegister;
            if (isRegister)
            {
            }
        }
        
        EventManager.Instance.Emit(EventId.FIRST_LOGIN_SERVER_COMPLETE, isRegister);
        NetManager.Instance.Send(RequestId.LOGIN_OVER, null, pack => { });
    }
}