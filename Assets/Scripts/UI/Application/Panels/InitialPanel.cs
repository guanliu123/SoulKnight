using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class InitialPanel : BasePanel
{
    
    public InitialPanel() : base(new UIType(UIInfo.InitialPanel))
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        
        FindComponent<Button>("Btn_Start").onClick.AddListener(() =>
        {
            LogTool.Log("开始游戏");
        });
        GameInit();
    }

    public void GameInit()
    {
        MonoManager.Instance.Init();
        //TableManager.Instance.Init();
        //PayManager.Instance.Init();
        NetReciver.Instance.Init(new ResponseRegister());
    }

    public void NetInit()
    {
        //这里后面要把host配置一下
        NetManager.Instance.Init();
        NetManager.Instance.Connect("");
    }
}
