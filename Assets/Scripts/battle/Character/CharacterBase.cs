using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase
{
    public GameObject gameObject { get; protected set; }
    public Transform transform => gameObject.transform;
    public CharacterRoot characterRoot { get; private set; }

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

    public CharacterBase(GameObject obj)
    {
        gameObject = obj;
        characterRoot = gameObject.GetComponent<CharacterRoot>();
        if (characterRoot == null)
        {
            LogTool.LogError($"角色身上未挂载CharacterRoot！");
        }
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
