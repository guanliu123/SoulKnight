using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    public int NowLevel { get; private set; }

    public override void Init()
    {
        base.Init();
        NowLevel = 1;
        
        MapManager.Instance.GenerateMap();
    }
}
