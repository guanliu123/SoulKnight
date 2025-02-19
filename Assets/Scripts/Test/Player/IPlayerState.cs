using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPlayerState : IState   // Start is called before the first frame update
{
    protected Animator animator;
    protected Rigidbody2D rigidBody;
    protected IPlayer player;
    
    public new PlayerStateMachine Machine{
        get=>base.Machine as PlayerStateMachine;
        set => base.Machine = value;
    }

    public IPlayerState(PlayerStateMachine machine) : base(machine)
    {
        
    }

    public override void OnInit()
    {
        base.OnInit();
        player = Machine.Player;
        animator = player.animator;
        rigidBody = player.rigidBody;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }
}
