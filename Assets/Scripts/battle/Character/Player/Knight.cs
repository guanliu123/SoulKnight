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
        //LogTool.Log("角色互动物品！物品信息:"+(InteractiveObjectType)info[0]);
        InteractiveObjectType objType = (InteractiveObjectType)info[0];
        if (objType == InteractiveObjectType.Weapon)
        {
            InteractiveObjectRoot root = (InteractiveObjectRoot)info[1];
            playerWeapons.Add(WeaponFactory.Instance.GetPlayerWeapon(root.gameObject,this));
            root.enabled = false;
        }
    }
}
