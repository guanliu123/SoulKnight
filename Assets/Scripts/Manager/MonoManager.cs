
#region

using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

#endregion

public class MonoManager : SingletonBase<MonoManager>
{
    private readonly MonoController controller;
    
    public MonoManager()
    {
        if (!controller)
        {
            //通过单例模式构造函数在场景中创建唯一的公共Mono模块控制器
            GameObject obj = new("MonoController");
            controller = obj.AddComponent<MonoController>();
        }
    }
    
    public void Init()
    {
    }
    
    /// <summary>
    ///     使用公共Mono模块添加帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void AddUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中添加更新函数
        controller.AddUpdateAction(fun);
    }
    
    /// <summary>
    ///     使用公共Mono模块移除帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void RemoveUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中移除更新函数
        controller.RemoveUpdateAction(fun);
    }
    
    public bool ContainsUpdateAction(Action fun)
    {
        return controller.ContainsUpdateAction(fun);
    }
    
    /// <summary>
    ///     使用公共Mono模块添加帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void AddFixedUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中添加更新函数
        controller.AddFixedUpdateAction(fun);
    }
    
    /// <summary>
    ///     使用公共Mono模块移除帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void RemoveFixedUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中移除更新函数
        controller.RemoveFixedUpdateAction(fun);
    }
    
    /// <summary>
    ///     使用公共Mono模块添加帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void AddLateUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中添加更新函数
        controller.AddLateUpdateAction(fun);
    }
    
    /// <summary>
    ///     使用公共Mono模块移除帧更新函数
    /// </summary>
    /// <param name="fun">对应的每帧所需执行的方法</param>
    public void RemoveLateUpdateAction(Action fun)
    {
        //向公共Mono模块控制器中移除更新函数
        controller.RemoveLateUpdateAction(fun);
    }
    
    //添加更新函数
    public void AddUpdate100msAction(Action fun)
    {
        controller.AddUpdate100msAction(fun);
    }
    
    //移除更新函数
    public void RemoveUpdate100msAction(Action fun)
    {
        controller.RemoveUpdate100msAction(fun);
    }
    
    //添加更新函数
    public void AddUpdate1000msAction(Action fun)
    {
        controller.AddUpdate1000msAction(fun);
    }
    
    //移除更新函数
    public void RemoveUpdate1000msAction(Action fun)
    {
        controller.RemoveUpdate1000msAction(fun);
    }
    
    // 提供一个公共方法来调度延迟的Action
    public void ScheduleActionForNextFrame(Action action)
    {
        controller.ScheduleActionForNextFrame(action);
    }
    
    /// <summary>
    ///     通过公共Mono模块开启协程
    /// </summary>
    /// <param name="fun">所需开启协程的迭代器</param>
    
    #region 协程相关接口封装
    
    public Coroutine StartCoroutine(string methodName)
    {
        return controller.StartCoroutine(methodName);
    }
    
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }
    
    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return controller.StartCoroutine(methodName, value);
    }
    
    public void StopAllCoroutines()
    {
        controller.StopAllCoroutines();
    }
    
    public void StopCoroutine(IEnumerator routine)
    {
        controller.StopCoroutine(routine);
    }
    
    public void StopCoroutine(string methodName)
    {
        controller.StopCoroutine(methodName);
    }
    
    #endregion
}