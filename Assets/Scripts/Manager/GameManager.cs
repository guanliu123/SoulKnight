using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnumCenter;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;

public class GameManager : MonoSingletonBase<GameManager>
{
    public GameModeType GameMode { get; private set; }
    
    //private AbstractManager abstracterManager;

    public override void AwakeInit()
    {
        base.AwakeInit();
        MonoManager.Instance.AddUpdateAction(() =>
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                AbstractManager.Instance.RegisterPlayerAbstract();
            }
        });
        
        MonoManager.Instance.AddUpdateAction(AbstractManager.Instance.OnUpdate);
    }

    public void SetGameMode(GameModeType mode)
    {
        GameMode = mode;
    }
}
