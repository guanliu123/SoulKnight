using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachineBase
{
    public PlayerBase Player { get; protected set; }

    public PlayerStateMachine(PlayerBase player) : base()
    {
        Player = player;
    }
}
