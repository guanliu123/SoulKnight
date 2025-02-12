using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCenter : SingletonBase<TimeCenter>
{
    
    private static long TimeDelay { get; set; }

    public static long TimeNow => TimeDelay + TimeStamp;
    public static long ServerTime
    {
        set => TimeDelay = value - TimeStamp;
        
        get => TimeDelay + TimeStamp;
    }
    private static long TimeStamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
    //时间戳毫秒
    public static long TimeStampMil => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    
    /// <summary>
    ///     明天零点
    /// </summary>
    public static long TomorrowZeroTime => TodayZeroTime + 86400;
    public static long TodayZeroTime
    {
        get
        {
            long time = TimeNow - TimeNow % 86400;
            return time;
        }
    }
    
    public static long NextWeekZeroTime
    {
        get
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
            long lTime = TodayZeroTime * 10000000;
            TimeSpan toNow = new(lTime);
            DateTime curTime = dtStart.Add(toNow);
            int day = Convert.ToInt32(curTime.DayOfWeek);
            day = day == 0 ? 7 : day;
            DateTime startTimeNextWeek = curTime.AddDays(1 - day + 7);
            long timeStamp = Convert.ToInt64((startTimeNextWeek - dtStart).TotalSeconds);
            return timeStamp;
        }
    }
}
