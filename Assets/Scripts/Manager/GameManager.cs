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
    private List<AbstractController> controllers;

    // public void Init(Transform[] rp)
    // {
    //     
    // }

    public override void AwakeInit()
    {
        base.AwakeInit();
        controllers = new();
        MonoManager.Instance.AddUpdateAction(() =>
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestPlayer();
            }
        });

        RegisterController();
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

    public void RegisterController()
    {
        InputController inputController = new();
        MonoManager.Instance.AddUpdateAction(inputController.OnUpdate);
        controllers.Add(inputController);
    }

    public T GetController<T>() where T : AbstractController
    {
        AbstractController system = controllers.Where(controller => controller is T).ToArray()[0];
        if (system != null) return system as T;
        return default(T);
    }
}
