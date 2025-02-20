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
    }

    protected override void AlwaysUpdate()
    {
        base.AlwaysUpdate();
        MainPlayer.OnUpdate();
    }
}
