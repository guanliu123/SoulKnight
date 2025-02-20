using System;
using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class InteractiveObjectRoot : MonoBehaviour
{
    public InteractiveObjectType type;

    public GameObject itemIndicator;

    public Collider2D collider;

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
        //todo:改成虚拟摇杆操作ETCInput.xxx
        if (collider!=null&&Input.GetKeyDown(KeyCode.F))
        {
            //发送互动事件
            EventManager.Instance.Emit(EventId.ON_INTERACTING_OBJECT,new object[]{this.type,this});
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            collider = other;
            itemIndicator.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            collider = null;
            itemIndicator.SetActive(false);
        }
    }
}
