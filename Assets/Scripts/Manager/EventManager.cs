using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventInfo { }
// 实现一个参数
public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}
// 实现无参
public class EventInfo : IEventInfo
{
    public UnityAction actions;
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

public class EventManager : SingletonBase<EventManager>
{
    // key对应事件的名字，value对应的是监听这个事件对应的委托函数
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    // 添加事件监听，一个参数的
    public void On<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions += action;
        else
            eventDic.Add(name, new EventInfo<T>(action));
    }

    // 添加事件监听，无参数的
    public void On(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions += action;
        else
            eventDic.Add(name, new EventInfo(action));
    }

    // 事件触发，无参的
    public void Emit(string name)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions?.Invoke();
        }
        else
        {
            Debug.LogWarning("Event named ["+name+"] not found!");
        }
    }

    //事件触发，一个参数的
    public void Emit<T>(string name, T info)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions?.Invoke(info);
    }

    //移除监听，无参的
    public void Off(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions -= action;
    }

    //移除监听，一个参数的
    public void Off<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }

    //清空某一类型的所有事件
    public void Clear(string eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
        }
    }
    
    // 清空事件中心，主要用在场景切换时
    public void Clear()
    {
        eventDic.Clear();
    }
}