using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase
{
    public StateMachineBase Machine { get; protected set; }
    private bool isInit;
    
    public StateBase(StateMachineBase machine)
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
