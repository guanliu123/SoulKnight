using System;
using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    //private static string uiInfo = "Prefabs/Panels/StartPanel";
    
    public LoginPanel() : base(new UIType(UIInfo.LoginPanel))
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        FindComponent<Button>("Btn_Login").onClick.AddListener(OnLogin);
    }

    public void OnLogin()
    {
        LogTool.Log("登陆成功");
    }
}
