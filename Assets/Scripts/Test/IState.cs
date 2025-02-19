using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public IStateMachine Machine { get; protected set; }
    private bool isInit;
    
    public IState(IStateMachine machine)
    {
        Machine = machine;
    }
    
    public virtual void OnInit(){}
    public virtual void OnUpdate(){}

    public virtual void OnEnter()
    {
        if (!isInit)
        {
            isInit = true;
            OnInit();
        }
    }
    public virtual void OnExit(){}
}
