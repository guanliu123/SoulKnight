#region

using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

#endregion

public static class LogTool
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, UnityEngine.Object context)
    {
        Debug.Log(message, context);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, Color color)
    {
        message = string.Format("<b><color=#{0:X2}{1:X2}{2:X2}>{3}</color></b>\n", (byte)(color.r * 255),
            (byte)(color.g * 255), (byte)(color.b * 255), message);
        Debug.Log(message);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogFormat(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }
    
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogErrorFormat(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
    }
}