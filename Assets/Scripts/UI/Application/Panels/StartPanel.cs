using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    //private static string uiInfo = "Prefabs/Panels/StartPanel";
    
    public StartPanel() : base(new UIType(UIInfo.StartPanel))
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        FindComponent<Text>("Text_HL").text = "测试";
        FindComponent<Button>("Btn_HL").onClick.AddListener(() =>
        {
            PanelManager.Instance.Pop();
            PanelManager.Instance.Push(new BattlePanel());
        });
    }
}
