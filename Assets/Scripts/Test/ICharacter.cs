using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICharacter
{
    public GameObject gameObject { get; protected set; }
    public Transform transform => gameObject.transform;

    private bool isLeft;
    public bool IsLeft
    {
        get => isLeft;
        set
        {
            if (value)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            isLeft = value;
        }
    }
    private bool isInit;
    private bool isStart;
    private bool isShouldRemove;
    private bool isAlreadyRemove;

    public ICharacter(GameObject obj)
    {
        gameObject = obj;
    }

    public void OnUpdate()
    {
        if (!isInit)
        {
            isInit = true;
            OnInit();
        }
        OnCharacterUpdate();
    }
    
    protected virtual void OnInit(){}
    protected virtual void OnCharacterStart(){}

    protected virtual void OnCharacterUpdate()
    {
        if (!isStart)
        {
            isStart = true;
            OnCharacterStart();
        }
        
        
    }
    protected virtual void OnCharacterDieStart(){}
    protected virtual void OnCharacterDieUpdate(){}

    public void Remove()
    {
        isShouldRemove = true;
    }
}
