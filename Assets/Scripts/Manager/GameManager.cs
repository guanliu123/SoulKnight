using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;

public enum GameModeType
{
    SingleMode=1<<0,
    MultipleMode=1<<1,
}

public class GameManager : SingletonBase<GameManager>
{
    public static GameModeType GameMode { get; private set; }

    public void Init(Transform[] rp)
    {
        
    }

    public static void SetGameMode(GameModeType mode)
    {
        GameMode = mode;
    }
}
