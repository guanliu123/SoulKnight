
public class GlobalData : SingletonBase<GlobalData>
{
    public string BuildVersion = "1";
    public int? changeServerId = -1;
    public string changeServerIp = "";
    public string country = "";
    public string DeviceIDByLocal; //本机deviceID
    public string DeviceIDByServer = null; //服务器返回deveiceID
    public string GameIp = null;
    public string GameIpAcc = null;
    public string GameVersion = "0.0.1";
    public string HotUpdateVersion = "";
    public bool JumpServer = false;
    public bool PaySkip = false;
    public string Platform = "LOCAL";
    public string[] UpdateVer;
    public int UrlAccIdx = 0;
    public string[] UrlAppConfig;
    private string[] UrlChangeRole;
    private string[] UrlGetRoleList;
    public string UrlResource = "";
    private string[] UrlServerList;
    private string[] UrlServerListForRole;
    
    public string DeviceID
    {
        get
        {
            if (DeviceIDByServer != null)
            {
                return DeviceIDByServer;
            }
            
            if (DeviceIDByLocal != null)
            {
                return DeviceIDByLocal;
            }
            
            // DeviceIDByLocal = PlatformManager.PF.DeviceId;
            return DeviceIDByLocal;
        }
    }
    
    
    public void SetUrlServerList(string[] urls)
    {
        UrlServerList = urls;
    }
    
    public string GetUrlServerList()
    {
        string url = UrlServerList[UrlAccIdx];
        return url;
    }
    
    public void SetUrlServerListForRole(string[] urls)
    {
        UrlServerListForRole = urls;
    }
    
    public void SetUrlGetRoleList(string[] urls)
    {
        UrlGetRoleList = urls;
    }
    
    public void SetUrlChangeRole(string[] urls)
    {
        UrlChangeRole = urls;
    }
    
    public static string ShowGameVersion()
    {
        return Instance.GameVersion;
    }
}