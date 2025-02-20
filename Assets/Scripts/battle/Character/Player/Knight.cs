using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class Knight : PlayerBase
{
    public Knight(GameObject obj) : base(obj)
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        EventManager.Instance.On<object[]>(EventId.ON_INTERACTING_OBJECT,InteractingObject);
        stateMachine.ChangeState<NormalCharacterIdleState>();
    }

    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
        //ETCInput.GetAxis("Vertical")==0 && ETCInput.GetAxis("Horizontal")==0
        
    }

    private void InteractingObject(object[] info)
    {
        InteractiveObjectType objType = (InteractiveObjectType)info[0];
        if (objType == InteractiveObjectType.Weapon)
        {
            InteractiveObjectRoot root = (InteractiveObjectRoot)info[1];
            PickUpWeapon(root.gameObject);
            root.IsInteractable=false;
        }
    }
}
