using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : IStateMachine
{
    public IPlayer Player { get; protected set; }

    public PlayerStateMachine(IPlayer player) : base()
    {
        Player = player;
    }
}
