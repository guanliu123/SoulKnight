using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScene : SceneBase
{
    public override void Init()
    {
        base.Init();
        scenePath = SceneInfo.LevelScene;
        sceneName = "LevelScene";
        basePanel = new BattleInfoPanel();
    }
}
