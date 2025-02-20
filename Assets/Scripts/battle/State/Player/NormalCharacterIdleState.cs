using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//常规的角色状态机，用于可复用的角色
public class NormalCharacterIdleState : PlayerStateBase
{
    public NormalCharacterIdleState(PlayerStateMachine machine) : base(machine)
    {
        
    }

    public override void OnEnter()
    {
        base.OnEnter();
        animator.Play("Idle");
        rigidBody.velocity = FixVector2.Zero.ToVector2();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        CheckIsMove();
    }

    private void CheckIsMove()
    {
        if(Mathf.Abs(ETCInput.GetAxis("Horizontal"))>0||Mathf.Abs(ETCInput.GetAxis("Vertical"))>0)
        {
            Machine.ChangeState<NormalCharacterWalkState>();
        }
    }
}
