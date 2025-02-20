using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineBase
{
    //todo:gameobject换成状态类
    private Dictionary<Type, StateBase> stateDic;
    private StateBase currentState;

    public StateMachineBase()
    {
        stateDic = new Dictionary<Type, StateBase>();
    }

    public void ChangeState<T>()
    {
        if (!stateDic.ContainsKey(typeof(T)))
        {
            //通过反射创造对象
            stateDic.Add(typeof(T),(StateBase)Activator.CreateInstance(typeof(T),this));
        }

        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState?.OnExit();
        currentState = stateDic[typeof(T)];
        currentState.OnEnter();
    }

    public void StopCurrentState()
    {
        currentState?.OnExit();
    }

    public void StartCurrentState()
    {
        
    }

    public void OnUpdate()
    {
        currentState?.OnUpdate();
    }
}
