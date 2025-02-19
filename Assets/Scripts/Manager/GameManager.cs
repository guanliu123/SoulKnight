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

public class GameManager : MonoSingletonBase<GameManager>
{
    public GameModeType GameMode { get; private set; }

    // public void Init(Transform[] rp)
    // {
    //     
    // }

    public override void AwakeInit()
    {
        base.AwakeInit();
        MonoManager.Instance.AddUpdateAction(() =>
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestPlayer();
            }
        });
    }

    public void SetGameMode(GameModeType mode)
    {
        GameMode = mode;
    }

    public void TestPlayer()
    {
        var playerController = new PlayerController();
        MonoManager.Instance.AddUpdateAction(playerController.OnUpdate);
    }
}
