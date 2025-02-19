using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Timer
{
    public Fix64 originDuration;
    public Fix64 duration;
    //public bool isLoop;
    public int loopTimes;
    //public int maxLoopTimes;
    public UnityAction callback;

    //如果循环次数设置为-1则永远循环
    public Timer(Fix64 _duration, UnityAction _callback,int _loopTimes)
    {
        originDuration = _duration;
        duration = _duration;
        loopTimes=_loopTimes;
        //maxLoopTimes = _loopTimes;
        callback = _callback;
    }

    public Fix64 DecreaseTime()
    {
        duration -= Time.deltaTime;
        return duration;
    }

    /// <summary>
    /// 重置定时器时间，方便下一次循环计算
    /// </summary>
    public void ResetTime()
    {
        duration = originDuration;
    }
    
    /// <summary>
    /// 将定时器重置为未初始化状态
    /// </summary>
    public void Format()
    {
        originDuration = (Fix64)0;
        duration = (Fix64)0;
        callback = null;
        loopTimes = 0;
        //maxLoopTimes = 0;
    }
}

public class TimerManager : MonoSingletonBase<TimerManager>
{
    //等待加入循环的列表，在每次遍历后加入
    private static List<Timer> waitList;
    private static List<Timer> timers;
    //储存被创建出来但是被停用的timer方便再次使用
    private static List<Timer> timerPool;

    protected override void Awake()
    {
        base.Awake();
        waitList = new List<Timer>();
        timers = new List<Timer>();
        timerPool = new List<Timer>();
    }

    private void Update()
    {
        timers.AddRange(waitList);
        waitList.Clear();
        
        for (int i = 0; i < timers.Count; i++)
        {
            if (timers[i].DecreaseTime() <= (Fix64)0)
            {
                timers[i].callback?.Invoke();
                if (timers[i].loopTimes > 0) timers[i].loopTimes--;
                if (timers[i].loopTimes==0)
                {                    
                    timers[i].Format();
                    timerPool.Add(timers[i]);
                    timers.RemoveAt(i);
                }
                else
                {
                    timers[i].ResetTime();
                }
            }
        }
    }

    public static Timer GetTimer(Fix64 _duration,UnityAction _callback,int _loopTimes=1)
    {
        Timer timer;
        if (timerPool.Count > 0)
        {
            timer = timerPool[0];
            timer.originDuration = _duration;
            timer.duration = _duration;
            timer.callback = _callback;
            timer.loopTimes = _loopTimes;
            timerPool.RemoveAt(0);
        }
        else
        {
            timer = new Timer(_duration, _callback,_loopTimes);
        }

        return timer;
    }

    public static void AddTimer(Timer timer)
    {
        waitList.Add(timer);
    }

    public static Timer Register(float duration,UnityAction _callback,int _loopTimes=1)
    {
        Fix64 _duration = (Fix64)duration;
        Timer t = GetTimer(_duration, _callback, _loopTimes);
        AddTimer(t);
        return t;
    }

    /// <summary>
    /// 重置定时器
    /// </summary>
    /// <param name="timer"></param>
    public static void ResetTimer(Timer timer)
    {
        if(timer==null|| !timers.Contains(timer)) return;
        
        timer.ResetTime();
    }
    
    public static void  RemoveTimer(Timer timer)
    {
        if(timer==null|| !timers.Contains(timer)) return;
        
        timer.duration = (Fix64)0;
        timer.callback=null;
        timer.loopTimes = 0;
    }
}
