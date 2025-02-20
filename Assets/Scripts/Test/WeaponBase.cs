using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class WeaponBase
{
    public GameObject gameObject { get; private set; }
    public Transform transform => gameObject.transform;
    protected CharacterBase character;

    private bool isInit;
    private bool isEnter;
    
    public WeaponBase(GameObject _obj, CharacterBase _character)
    {
        gameObject = _obj;
        character = _character;
    }

    protected virtual void OnInit()
    {
        
    }
    
    public virtual void OnEnter()
    {
        if (!isInit)
        {
            isInit = true;
            OnInit();
        }
    }

    protected virtual void OnFire()//发射时执行一次
    {
        
    }

    public virtual void OnExit()
    {
        isEnter = false;
    }

    public void OnUpdate()
    {
        if (!isEnter)
        {
            isEnter = true;
            OnEnter();
        }
    }
}
