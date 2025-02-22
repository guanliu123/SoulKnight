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
