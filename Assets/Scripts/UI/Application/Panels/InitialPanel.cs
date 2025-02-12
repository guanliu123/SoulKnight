using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

public class InitialPanel : BasePanel
{
    
    public InitialPanel() : base(new UIType(UIInfo.InitialPanel))
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        
        //FindComponent<Button>("Btn_Login").onClick.AddListener(OnLogin);
        //这里进行一些游戏的初始化操作，包括重要的manager初始化和网络连接
        GameInit();
    }

    public void GameInit()
    {
        LoadManager.Instance.Init();
        NetManager.Instance.Init();
        //这里后面要把host配置一下
        NetManager.Instance.Connect("");
        MonoManager.Instance.Init();
        //TableManager.Instance.Init();
        //PayManager.Instance.Init();
        NetReciver.Instance.Init(new ResponseRegister());
    }
}
