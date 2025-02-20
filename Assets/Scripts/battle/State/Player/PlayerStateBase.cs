using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase : StateBase   // Start is called before the first frame update
{
    protected Animator animator;
    protected Rigidbody2D rigidBody;
    protected PlayerBase player;
    
    public new PlayerStateMachine Machine{
        get=>base.Machine as PlayerStateMachine;
        set => base.Machine = value;
    }

    public PlayerStateBase(PlayerStateMachine machine) : base(machine)
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
