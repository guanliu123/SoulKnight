using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EnumCenter;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class InitialPanel : BasePanel
{
    private Vector3 dir = Vector3.up;
    private float distrance = 110f;
    private float durationTime=0.5f;
    private Image scarf;
    private Transform tips;
    private bool isShow;
    
    public InitialPanel() : base(new UIType(UIInfo.InitialPanel))
    {
        //todo:后面删掉
        UserData.UpdateUserData();
    }
    public override void OnEnter()
    {
        base.OnEnter();
        
        Button btnMain=FindComponent<Button>("Btn_Main");
        scarf = FindComponent<Image>("Img_Scarf");
        tips = FindComponent<Transform>("BottomTips");

        btnMain.onClick.AddListener(() =>
        {
            btnMain.interactable = false;
            Transform btnGroup=FindComponent<Transform>("BottomBtnGroup");
            isShow = !isShow;
            ChangeImg();
            btnGroup.DOMove(dir*distrance+btnGroup.position, durationTime).OnComplete(() =>
            {
                dir = -dir;
                btnMain.interactable = true;
            });
        });
        GameInit();
        FindComponent<Button>("Btn_SinglePerson").onClick.AddListener(() =>
        {
            GameManager.Instance.SetGameMode(GameModeType.SingleMode);
            GameRoot.Instance.SwitchScene(new HomeScene());
            // if (UserData.isNew)
            // {
            //     GameRoot.Instance.SwitchScene(new LevelScene());
            // }
            // else
            // {
            //     GameRoot.Instance.SwitchScene(new HomeScene());
            // }
        });
    }

    public void ChangeImg()
    {
        tips.gameObject.SetActive(!isShow);
        scarf.DOFade(isShow?1:0, durationTime);
    }

    public void GameInit()
    {
        MonoManager.Instance.Init();
    }

    public void NetInit()
    {
        //这里后面要把host配置一下
        NetManager.Instance.Init();
        NetManager.Instance.Connect("");
        NetReciver.Instance.Init(new ResponseRegister());
    }
}
