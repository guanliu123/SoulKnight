using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScene : SceneBase
{
    public override void Init()
    {
        base.Init();
        scenePath = SceneInfo.HomeScene;
        sceneName = "HomeScene";
    }

    protected override void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.SceneLoaded(scene, mode);
        //GameManager.Instance.TestPlayer();
    }
}
