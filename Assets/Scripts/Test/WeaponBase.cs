using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class WeaponBase
{
    public GameObject gameObject { get; private set; }
    public Transform transform => gameObject.transform;
    protected CharacterBase character;
    
    //武器能否被旋转
    protected bool canRotate;

    private bool isInit;
    private bool isEnter;
    
    public WeaponBase(GameObject _obj, CharacterBase _character)
    {
        gameObject = _obj;
        character = _character;
        //一般大部分武器都能旋转
        canRotate = true;
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
        LogTool.Log("武器发射一次");
    }

    public virtual void OnExit()
    {
        isEnter = false;
    }

    public virtual void OnDestory()
    {
        
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
