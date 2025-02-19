using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : IPlayer
{
    public Knight(GameObject obj) : base(obj)
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        stateMachine.ChangeState<NormalCharacterIdleState>();
    }

    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        //ETCInput.GetAxis("Vertical")==0 && ETCInput.GetAxis("Horizontal")==0
        
    }
}
