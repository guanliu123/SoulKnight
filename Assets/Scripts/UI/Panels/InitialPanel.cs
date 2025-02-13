using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class InitialPanel : BasePanel
{
    private Vector3 dir = Vector3.up;
    private float distrance = 100f;
    private float durationTime=0.5f;
    
    public InitialPanel() : base(new UIType(UIInfo.InitialPanel))
    {
    }
    public override void OnEnter()
    {
        base.OnEnter();
        
        Button btnMain=FindComponent<Button>("Btn_Main");
        btnMain.onClick.AddListener(() =>
        {
            btnMain.interactable = false;
            Transform btnGroup=FindComponent<Transform>("BottomBtnGroup");
            btnGroup.DOMove(dir*distrance+btnGroup.position, durationTime).OnComplete(() =>
            {
                dir = -dir;
                btnMain.interactable = true;
            });
        });
        GameInit();
        FindComponent<Button>("Btn_SinglePerson").onClick.AddListener(() =>
        {
            GameManager.SetGameMode(GameModeType.SingleMode);
            GameRoot.Instance.SwitchScene(SceneInfo.HomeScene);
        });
    }

    public void ChangeImg(bool isShow)
    {
        //todo:显示围巾和标题
    }

    public void GameInit()
    {
        MonoManager.Instance.Init();
        //PayManager.Instance.Init();
    }

    public void NetInit()
    {
        //这里后面要把host配置一下
        NetManager.Instance.Init();
        NetManager.Instance.Connect("");
        NetReciver.Instance.Init(new ResponseRegister());
    }
}
