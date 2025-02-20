using System;
using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class InteractiveObjectRoot : MonoBehaviour
{
    public InteractiveObjectType type;

    public GameObject itemIndicator;

    private Collider2D collider;

    private bool isInteractable;

    public bool IsInteractable
    {
        get
        {
            return isInteractable;
        }
        set
        {
            if (!value)
            {
                collider = null;
            }

            this.enabled = value;
            isInteractable = value;
        }
    }

    private void Awake()
    {
        IsInteractable=true;
    }

    private void OnEnable()
    {
        itemIndicator.SetActive(false);
    }

    private void OnDisable()
    {
        itemIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!isInteractable) return;
        //todo:改成虚拟摇杆操作ETCInput.xxx
        if (collider!=null&&Input.GetKeyDown(KeyCode.F))
        {
            //发送互动事件
            EventManager.Instance.Emit(EventId.ON_INTERACTING_OBJECT,new object[]{this.type,this});
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable) return;

        if (other.CompareTag("Player"))
        {
            collider = other;
            itemIndicator.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isInteractable) return;

        if (other.CompareTag("Player"))
        {
            collider = null;
            itemIndicator.SetActive(false);
        }
    }
}
