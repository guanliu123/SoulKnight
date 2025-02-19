using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStateMachine
{
    //todo:gameobject换成状态类
    private Dictionary<Type, IState> stateDic;
    private IState currentState;

    public IStateMachine()
    {
        stateDic = new Dictionary<Type, IState>();
    }

    public void ChangeState<T>()
    {
        if (!stateDic.ContainsKey(typeof(T)))
        {
            //通过反射创造对象
            stateDic.Add(typeof(T),(IState)Activator.CreateInstance(typeof(T),this));
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
