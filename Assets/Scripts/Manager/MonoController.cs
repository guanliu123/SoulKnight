
#region

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class MonoController : MonoBehaviour
{
    private readonly float timerInterval = 0.1f;
    private float _timer;
    private bool isPause;
    private long pauseStartTime;
    private int timerCal;
    
    
    //存储需要每帧更新的方法们
    private event Action updateAction;
    private event Action fixedUpdateAction;
    private event Action lateUpdateAction;
    private event Action onUpdate100msAction;
    private event Action onUpdate1000msAction;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        _timer = timerInterval;
        // GameSceneManager.Instance.Controller = m_SceneStateController;
        // m_SceneStateController.SetState(new LoginState(m_SceneStateController), "");
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        isPause = !hasFocus;
        CheckApplicationStatus();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        isPause = pauseStatus;
        CheckApplicationStatus();
    }
    
    private void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
    
    private void CheckApplicationStatus()
    {
        if (isPause)
        {
            // 获取当前时间
            DateTime now = DateTime.UtcNow;
            pauseStartTime = (long)(now - new DateTime(1970, 1, 1)).TotalSeconds;
            EventManager.Instance.Emit(EventId.ON_APPLICATION_PAUSE);
        }
        else
        {
            DateTime now = DateTime.UtcNow;
            long sec = (long)(now - new DateTime(1970, 1, 1)).TotalSeconds;
            long gap = sec - pauseStartTime;
            if (gap < 0)
            {
                gap = 0;
            }
            
            EventManager.Instance.Emit(EventId.ON_APPLICATION_RESUME, gap);
        }
    }
    
    private void OnTimer100ms()
    {
        timerCal++;
        OnUpdate100ms();
        if (timerCal >= 10)
        {
            timerCal = 0;
            OnUpdate1000ms();
        }
    }
    
    private void OnUpdate100ms()
    {
        if (onUpdate100msAction != null)
        {
            onUpdate100msAction();
        }
    }
    
    private int secAccum;
    
    private void OnUpdate1000ms()
    {
        // if (UserManager.Instance.UID != "")
        // {
        //     secAccum++;
        //     if (secAccum >= 60)
        //     {
        //         secAccum = 0;
        //         int acc = LocalStorage.GetStorage<int>(LocalStorageId.GameTimeAccum, true) ?? 0;
        //         acc++;
        //         if (acc == 30)
        //         {
        //             Dictionary<string, string> eventValues = new();
        //             eventValues.Add("Min", acc + "");
        //             AppsFlyerManager.Instance.SendCustomEvent(AppsFlyerEventId.GameTimeTotal, eventValues);
        //         }
        //         
        //         LocalStorage.SetStorage(LocalStorageId.GameTimeAccum, acc, true);
        //     }
        // }
        
        if (onUpdate1000msAction != null)
        {
            onUpdate1000msAction();
        }
    }
    
    private void Update()
    {
        if (!Application.isFocused)
        {
            return;
        }
        
        if (updateAction != null)
        {
            updateAction();
        }
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            OnTimer100ms();
            _timer = timerInterval;
        }
    }
    
    private void LateUpdate()
    {
        if (!Application.isFocused)
        {
            return;
        }
        
        if (lateUpdateAction != null)
        {
            lateUpdateAction();
        }
    }
    
    //添加更新函数
    public void AddUpdateAction(Action fun)
    {
        if (ContainsUpdateAction(fun))
        {
            LogTool.LogError("MonoController AddUpdateAction : Action has bean added");
            return;
        }
        
        updateAction += fun;
    }
    
    //移除更新函数
    public void RemoveUpdateAction(Action fun)
    {
        updateAction -= fun;
    }
    
    public bool ContainsUpdateAction(Action fun)
    {
        if (updateAction == null)
        {
            return false;
        }
        
        return updateAction.GetInvocationList().Any(d => Delegate.Equals(d, fun));
    }
    
    private void FixedUpdate()
    {
        if (fixedUpdateAction != null)
        {
            fixedUpdateAction();
        }
    }
    
    //添加更新函数
    public void AddFixedUpdateAction(Action fun)
    {
        if (ContainsFixedUpdateAction(fun))
        {
            LogTool.LogError("MonoController AddFixedUpdateAction : Action has bean added");
            return;
        }
        
        fixedUpdateAction += fun;
    }
    
    //移除更新函数
    public void RemoveFixedUpdateAction(Action fun)
    {
        fixedUpdateAction -= fun;
    }
    
    public bool ContainsFixedUpdateAction(Action fun)
    {
        if (fixedUpdateAction == null)
        {
            return false;
        }
        
        return fixedUpdateAction.GetInvocationList().Any(d => Delegate.Equals(d, fun));
    }
    
    public void AddLateUpdateAction(Action fun)
    {
        if (ContainsLateUpdateAction(fun))
        {
            LogTool.LogError("MonoController AddLateUpdateAction : Action has bean added");
            return;
        }
        
        lateUpdateAction += fun;
    }
    
    //移除更新函数
    public void RemoveLateUpdateAction(Action fun)
    {
        lateUpdateAction -= fun;
    }
    
    public bool ContainsLateUpdateAction(Action fun)
    {
        if (lateUpdateAction == null)
        {
            return false;
        }
        
        return lateUpdateAction.GetInvocationList().Any(d => Delegate.Equals(d, fun));
    }
    
    //添加更新函数
    public void AddUpdate100msAction(Action fun)
    {
        onUpdate100msAction += fun;
    }
    
    //移除更新函数
    public void RemoveUpdate100msAction(Action fun)
    {
        onUpdate100msAction -= fun;
    }
    
    //添加更新函数
    public void AddUpdate1000msAction(Action fun)
    {
        onUpdate1000msAction += fun;
    }
    
    //移除更新函数
    public void RemoveUpdate1000msAction(Action fun)
    {
        onUpdate1000msAction -= fun;
    }
    
    // 提供一个公共方法来调度延迟的Action
    public void ScheduleActionForNextFrame(Action action)
    {
        StartCoroutine(ExecuteActionNextFrame(action));
    }
    
    private IEnumerator ExecuteActionNextFrame(Action action)
    {
        // 等待下一帧
        yield return new WaitForFixedUpdate();
        action?.Invoke();
    }
}