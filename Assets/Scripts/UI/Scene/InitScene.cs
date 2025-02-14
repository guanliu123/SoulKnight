using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
public class InitScene : SceneBase
{
    public override void Init()
    {
        base.Init();
        scenePath = SceneInfo.InitScene;
        sceneName = "InitScene";
        basePanel = new InitialPanel();
    }
}
