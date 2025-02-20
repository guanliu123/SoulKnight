using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCharacterWalkState : PlayerStateBase
{
    public NormalCharacterWalkState(PlayerStateMachine machine) : base(machine)
    {
        
    }

    public override void OnEnter()
    {
        base.OnEnter();
        animator.Play("Walk");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        GetPlayerInputToMove();
    }

    private void GetPlayerInputToMove()
    {
        Fix64 hor =(Fix64)ETCInput.GetAxis("Horizontal");
        Fix64 ver =(Fix64)ETCInput.GetAxis("Vertical");
        FixVector2 moveDir = new FixVector2(hor, ver);

        if (FixVector2.Magnitude(moveDir) > 0)
        {
            moveDir.Normalize();
            rigidBody.transform.position += (Vector3)(moveDir * (Fix64)6 * (Fix64)Time.deltaTime).ToVector2();
            player.IsLeft = hor < 0;
        }
        else
        {
            Machine.ChangeState<NormalCharacterIdleState>();
        }
    }
}
