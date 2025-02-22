using System;
using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;
using HedgehogTeam.EasyTouch;

public class PlayerController : AbstractController
{
    public PlayerBase MainPlayer { get; private set; }
    public PlayerController(){}

    protected override void Init()
    {
        base.Init();
        MainPlayer = PlayerFactory.Instance.GetPlayer(PlayerType.Knight);
        MainPlayer.SetInput(AbstractManager.Instance.GetController<InputController>().input);
    }

    protected override void AlwaysUpdate()
    {
        base.AlwaysUpdate();
        if(MainPlayer!=null) MainPlayer.OnUpdate();
    }
}
