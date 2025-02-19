using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelScene : SceneBase
{
    public override void Init()
    {
        base.Init();
        scenePath = SceneInfo.LevelScene;
        sceneName = "LevelScene";
        basePanel = new LoadingPanel();
    }

    protected override void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.SceneLoaded(scene, mode);
        LevelManager.Instance.Init();
    }
}
