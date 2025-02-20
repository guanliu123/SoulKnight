#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#endregion

public static class GameTool
{
    #region 时间显示方法
    
    public static string GetLeftTimeShow(int remainingSeconds)
    {
        // 计算小时、分钟和秒
        int hours = remainingSeconds / 3600;
        int minutes = remainingSeconds % 3600 / 60;
        int seconds = remainingSeconds % 60;
        
        // 格式化为hh:mm:ss格式
        string formattedTime = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        
        // 输出结果
        return formattedTime;
    }
    
    
    /// <summary>
    ///     ＜60秒，单位:秒 S
    ///     61-3600秒，单位:分 M
    ///     3601-86400秒，单位:时 H
    ///     ＞86400秒，单位：天 D
    /// </summary>
    /// <param name="cd"></param>
    /// <returns></returns>
    public static string TimeShowSingleCD(long cd)
    {
        string showStr = "";
        if (cd > 86400)
        {
            int day = Mathf.FloorToInt(cd / 86400f);
            showStr = day + "D";
        }
        else if (cd > 3600)
        {
            int hour = Mathf.FloorToInt(cd / 3600f);
            showStr = hour + "H";
        }
        else if (cd > 60)
        {
            int min = Mathf.FloorToInt(cd / 60f);
            showStr = min + "M";
        }
        else if (cd >= 0)
        {
            showStr = cd + "S";
        }
        
        return showStr;
    }
    
    #endregion
    
    
    /// <summary>
    ///  检查是否存在属性
    /// </summary>
    public static bool IsHaveProperty(Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);
        return property != null;
    }
    
    public static void ConvertJObjectToClass(object classObj, JObject obj)
    {
        string[] keys = obj.Properties().Select(item => item.Name).ToArray();
        Type classType = classObj.GetType();
        foreach (string key in keys)
        {
            //FieldInfo field = _loginType.GetField(key);
            PropertyInfo prop = classType.GetProperty(key);
            if (prop == null || !prop.CanWrite)
            {
                continue;
            }
            
            Type type = prop.PropertyType;
            if (type == typeof(string))
            {
                prop.SetValue(classObj, (string)obj.GetValue(key));
            }
            else if (type == typeof(long))
            {
                prop.SetValue(classObj, (long)obj.GetValue(key));
            }
            else if (type == typeof(float))
            {
                prop.SetValue(classObj, (float)obj.GetValue(key));
            }
            else if (type == typeof(int))
            {
                prop.SetValue(classObj, (int)obj.GetValue(key));
            }
            else if (type == typeof(short))
            {
                prop.SetValue(classObj, (int)obj.GetValue(key));
            }
            else if (type == typeof(bool))
            {
                prop.SetValue(classObj, (bool)obj.GetValue(key));
            }
            else if (type == typeof(object))
            {
                prop.SetValue(classObj, obj.GetValue(key));
            }
            else if (type == typeof(int[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<int[]>());
            }
            else if (type == typeof(string[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<string[]>());
            }
            else if (type == typeof(float[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<float[]>());
            }
            else if (type == typeof(long[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<long[]>());
            }
            else if (type == typeof(bool[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<bool[]>());
            }
            else if (type == typeof(object[]))
            {
                JArray jsonArray = (JArray)obj.GetValue(key);
                prop.SetValue(classObj, jsonArray?.ToObject<object[]>());
            }
            // else if (type == typeof(TATarget[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TATarget[]>());
            // }
            // else if (type == typeof(TUserDino[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TUserDino[]>());
            // }
            // else if (type == typeof(TUserDinoNest[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TUserDinoNest[]>());
            // }
            // else if (type == typeof(TUserDinoEgg[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TUserDinoEgg[]>());
            // }
            // else if (type == typeof(TUserDinoEquip[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TUserDinoEquip[]>());
            // }
            // else if (type == typeof(TUserDinoBaby[]))
            // {
            //     JArray jsonArray = (JArray)obj.GetValue(key);
            //     prop.SetValue(classObj, jsonArray?.ToObject<TUserDinoBaby[]>());
            // }
            // else if (type == typeof(TResources))
            // {
            //     prop.SetValue(classObj, obj.GetValue(key)?.ToObject<TResources>());
            // }
            // else if (type == typeof(TUserDinoHatcher))
            // {
            //     prop.SetValue(classObj, obj.GetValue(key)?.ToObject<TUserDinoHatcher>());
            // }
            else
            {
                Type aaa = obj.GetValue(key)?.GetType();
                LogTool.LogError("ConvertJObjectToClass undisposed Type:" + aaa);
            }
        }
    }
    
    private static Vector2 ParseVector2(JToken token)
    {
        if (token == null || token.Type != JTokenType.Object)
        {
            return Vector2.zero; // 或者其他默认值
        }
        
        float x = token.Value<float>("x");
        float y = token.Value<float>("y");
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// 从一个物体上找到某个子物体（慎用尤其是在Update里！）
    /// </summary>
    /// <param name="parent">父物体</param>
    /// <param name="name">要找的子物体名称</param>
    /// <returns></returns>
    public static GameObject GetGameObjectFromChildren(GameObject parent, string name)
    {
        foreach (Transform obj in parent.GetComponentsInChildren<Transform>())
        {
            if (obj.name == name)
            {
                return obj.gameObject;
            }
        }
        LogTool.Log("GameTool GetGameObjectFromChildren(" + parent.name + " " + name + ") return null");
        return null;
    }
    
    /// <summary>
    ///     在设置完UI位置后使用,检测UI是否超出屏幕并设置UI在屏幕内
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="isChange"></param>
    public static Vector3 SetUIInScreen(RectTransform rectTransform, bool isChange = true)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        Vector3 worldPos = rectTransform.transform.position;
        float xOffset = 0;
        float yOffset = 0;
        Rect rect = rectTransform.rect;
        Vector2 pivot = rectTransform.pivot;
        float left = worldPos.x - rect.width * pivot.x;
        float right = worldPos.x + rect.width * (1 - pivot.x);
        float top = worldPos.y + rect.height * (1 - pivot.y);
        float bottom = worldPos.y - rect.height * pivot.y;
        if (left < 0)
        {
            xOffset = -left;
        }
        
        if (right > Screen.width)
        {
            xOffset = Screen.width - right;
        }
        
        if (bottom < 0)
        {
            yOffset = -bottom;
        }
        
        if (top > Screen.height)
        {
            yOffset = Screen.height - top;
        }
        
        Vector3 offset = new(xOffset, yOffset, 0);
        if (isChange)
        {
            rectTransform.position += offset;
        }
        
        return offset;
    }
    
    public static bool IsPointerOverTargetUIObject(GameObject target)
    {
        if (target is null)
        {
            LogTool.LogError("传入的target为空!");
            return false;
        }
        
        PointerEventData eventDataCurrentPosition = new(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        foreach (RaycastResult result in results)
        {
            if (target.Equals(result.gameObject)) // 检查结果列表中是否包含我们的目标UI对象
            {
                return true; // 点击发生于目标UI对象之上
            }
        }
        
        return false; // 点击未发生于目标UI对象之上
    }
    
    /// <summary>
    ///     转换时间戳到DateTime
    /// </summary>
    /// <param name="timestamp">时间戳</param>
    /// <param name="millisecond">是否是毫秒时间戳</param>
    /// <returns></returns>
    public static DateTime GetTimeFromTimeStamp(long timestamp, bool millisecond = false)
    {
        string ID = TimeZoneInfo.Local.Id;
        DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
        DateTime res = millisecond ? startTime.AddMilliseconds(timestamp) : startTime.AddSeconds(timestamp);
        return res;
    }
    
    //时间戳转日期 秒
    public static string GetTimeShow(long timestamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1).AddSeconds(timestamp).ToLocalTime(); // 将时间戳转换为DateTime
        string dateString = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        return dateString;
    }
    
    /// <summary>
    ///     转换进制
    /// </summary>
    public static string ConvertScale(int value)
    {
        string res = value.ToString();
        //过万时转换为千
        if (value > 10000)
        {
            res = $"{value / 1000f:F1}K";
        }
        
        return res;
    }
    
    // public static void SetIconAndTextByCost(Image img, TMP_Text text, string costStr)
    // {
    //     string[] strs = costStr.Split("|");
    //     int id = Convert.ToInt32(strs[0]);
    //     Sprite sprite = LoadManager.Instance.Load<Sprite>(id > 100
    //         ? TableManager.Instance.Tables.TbItem[id].Icon
    //         : TableManager.Instance.Tables.TbRes[id].Icon
    //     );
    //     img.sprite = sprite;
    //     text.text = $"x{strs[1]}";
    // }
    
    /// <summary>
    ///     插入排序,稳定排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="comparison"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void InsertionSort<T>(IList<T> list, Comparison<T> comparison)
    {
        if (list == null)
        {
            throw new ArgumentNullException("list");
        }
        
        if (comparison == null)
        {
            throw new ArgumentNullException("comparison");
        }
        
        int count = list.Count;
        for (int j = 1; j < count; j++)
        {
            T key = list[j];
            
            int i = j - 1;
            for (; i >= 0 && comparison(list[i], key) > 0; i--)
            {
                list[i + 1] = list[i];
            }
            
            list[i + 1] = key;
        }
    }
    
    // public static T GetDataConfig<T>(int id)
    // {
    //     string _str = TableManager.Instance.Tables.TbDataConfig[id].Value;
    //     if (typeof(T) == typeof(string))
    //     {
    //         return (T)(object)_str;
    //     }
    //
    //     if (typeof(T) == typeof(int))
    //     {
    //         return (T)Convert.ChangeType(Convert.ToInt32(_str), typeof(T));
    //     }
    //
    //     if (typeof(T).IsEnum)
    //     {
    //         return (T)Enum.Parse(typeof(T), _str);
    //     }
    //
    //     return (T)(object)_str;
    // }
    
    #region Table字符串处理
    
    /// <summary>
    ///     "1001|1,1002|2" -> [[1001,1],[1002,2]]
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static List<List<int>> ConvertStringTo2DArray1(string str)
    {
        string[] strArr = str.Split(",");
        List<List<int>> list = new(strArr.Length);
        for (int i = 0; i < strArr.Length; i++)
        {
            if (strArr[i] == "")
            {
                break;
            }
            
            string[] arr = strArr[i].Split("|");
            list.Add(new List<int>());
            for (int j = 0; j < arr.Length; j++)
            {
                list[i].Add(int.Parse(arr[j]));
            }
        }
        
        return list;
    }
    
    /// <summary>
    ///     "1|2|3" -> [1,2,3]
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static List<int> ConvertStringTo1DArray1(string str)
    {
        List<int> ints = new();
        if (str.IsNullOrEmpty())
        {
            return ints;
        }
        
        string[] strs = str.Split("|");
        foreach (string item in strs)
        {
            ints.Add(int.Parse(item));
        }
        
        return ints;
    }
    
    
    public static string[] ConvertStringTo1DArray1WithString(string str)
    {
        string[] strArr = str.Split("|");
        return strArr;
    }
    
    public static List<List<string>> ConvertStringTo2DStringList(string str)
    {
        string[] strArr = str.Split(",");
        List<List<string>> list = new(strArr.Length);
        for (int i = 0; i < strArr.Length; i++)
        {
            if (strArr[i] == "")
            {
                break;
            }
            
            string[] arr = strArr[i].Split("|");
            list.Add(new List<string>());
            for (int j = 0; j < arr.Length; j++)
            {
                list[i].Add(arr[j]);
            }
        }
        
        return list;
    }
    
    public static List<List<List<int>>> ConvertStringTo3DStringList(string str)
    {
        // 按照第一级分隔符 ';' 分割字符串
        string[] firstLevelArr = str.Split(";");
        List<List<List<int>>> list3D = new(firstLevelArr.Length);
        
        for (int i = 0; i < firstLevelArr.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(firstLevelArr[i]))
            {
                continue;
            }
            
            // 按照第二级分隔符 ',' 分割字符串
            string[] secondLevelArr = firstLevelArr[i].Split(",");
            List<List<int>> list2D = new(secondLevelArr.Length);
            
            for (int j = 0; j < secondLevelArr.Length; j++)
            {
                if (string.IsNullOrWhiteSpace(secondLevelArr[j]))
                {
                    continue;
                }
                
                // 按照第三级分隔符 '|' 分割字符串
                string[] thirdLevelArr = secondLevelArr[j].Split("|");
                List<int> list1D = new(thirdLevelArr.Length);
                
                for (int k = 0; k < thirdLevelArr.Length; k++)
                {
                    list1D.Add(int.Parse(thirdLevelArr[k]));
                }
                
                list2D.Add(list1D);
            }
            
            list3D.Add(list2D);
        }
        
        return list3D;
    }
    
    #endregion
    
    public static int GetRandomIntByWeight(Dictionary<int, int> prop)
    {
        //计算总权重
        int totelWeight = 0;
        foreach (KeyValuePair<int, int> item in prop)
        {
            totelWeight += item.Value;
        }
        
        int randomValue = Random.Range(0, totelWeight);
        foreach (KeyValuePair<int, int> pair in prop)
        {
            if (randomValue < pair.Value)
            {
                return pair.Key;
            }
            
            randomValue -= pair.Value;
        }
        
        return 0;
    }
    
    public static int GetRandomIntByWeight(string config)
    {
        if (config == "")
        {
            return 0;
        }
        
        List<List<int>> propList = ConvertStringTo2DArray1(config);
        Dictionary<int, int> prop = new(propList.Count);
        for (int i = 0; i < propList.Count; i++)
        {
            prop.Add(propList[i][0], propList[i][1]);
        }
        
        int res = GetRandomIntByWeight(prop);
        return res;
    }
    
    /**
     * 不使用Split，List的GC比SPlit的GC要少
     * 2|3,2|-3,2|0,-2|0,0|3 返回 [[2,3],[2,-3],[2,0],[-2,0],[0,3]],
     * 2|3 返回 [[2,3]]
     * 1|2|3 返回[[1],[2],[3]]
    */
    public static int[][] ConvertStringTo2DArrayFast(string input)
    {
        // 最终数组的列表，每个元素是一个代表一组数字的数组
        List<int[]> result = new();
        
        // 开始和结束索引用于标记当前解析的数字
        int startIdx = 0;
        
        // 用于暂存每一组数字
        List<int> currentGroup = new();
        
        for (int i = 0; i < input.Length; i++)
        {
            // 当遇到分隔符或者达到字符串末尾时，解析数字
            if (input[i] == '|' || input[i] == ',' || i == input.Length - 1)
            {
                // 如果是字符串末尾，调整结束索引以包含最后一个字符
                int endIdx = i == input.Length - 1 ? i : i - 1;
                // 解析当前数字并添加到当前数字组
                if (endIdx >= startIdx)
                {
                    string numberStr = input[startIdx..(endIdx + 1)];
                    currentGroup.Add(int.Parse(numberStr));
                }
                
                // 如果遇到逗号或者字符串末尾，结束当前数字组的解析
                if (input[i] == ',' || i == input.Length - 1)
                {
                    result.Add(currentGroup.ToArray());
                    currentGroup = new List<int>(); // 开始一个新的数字组
                }
                
                // 更新开始索引到下一个字符
                startIdx = i + 1;
            }
            else if (!char.IsDigit(input[i]) && input[i] != '-' && input[i] != '+')
            {
                // 非数字字符，更新开始索引跳过该字符
                startIdx = i + 1;
            }
        }
        
        return result.ToArray();
    }
    
    /// <summary>
    ///     判断对象是否为 null 或空。
    /// </summary>
    /// <typeparam name="T">对象的类型。</typeparam>
    /// <param name="obj">要检查的对象。</param>
    /// <returns>如果对象为 null 或空则返回 true，否则返回 false。</returns>
    public static bool IsNullOrEmpty<T>(this T obj)
    {
        if (obj == null)
        {
            return true;
        }
        
        if (obj is string str)
        {
            return string.IsNullOrEmpty(str);
        }
        
        if (obj is ICollection collection)
        {
            return collection.Count == 0;
        }
        
        return false;
    }
    
    public static object GetObjectValue<T>(this T obj, string key)
    {
        if (obj != null)
        {
            PropertyInfo prop = obj.GetType().GetProperty(key);
            if (prop != null)
            {
                object value = prop.GetValue(obj);
                return value;
            }
        }
        
        return null;
    }
    
    // public static T GetDataConfig<T>(int id)
    // {
    //     string str = TableManager.Instance.Tables.TbDataConfig[id].Value;
    //     if (typeof(T) == typeof(string))
    //     {
    //         return (T)(object)str;
    //     }
    //     
    //     if (typeof(T) == typeof(int))
    //     {
    //         return (T)Convert.ChangeType(Convert.ToInt32(str), typeof(T));
    //     }
    //     
    //     if (typeof(T).IsEnum)
    //     {
    //         return (T)Enum.Parse(typeof(T), str);
    //     }
    //     
    //     return (T)(object)str;
    // }
    
    public static Vector3 GetRandomPosition(Vector3 center, Vector3 size, bool ignoreY = true)
    {
        float minX = center.x - size.x / 2;
        float maxX = center.x + size.x / 2;
        
        float minY = center.y - size.y / 2;
        float maxY = center.y + size.y / 2;
        
        float minZ = center.z - size.z / 2;
        float maxZ = center.z + size.z / 2;
        float randomY;
        if (ignoreY)
        {
            randomY = 0;
        }
        else
        {
            randomY = Random.Range(minY, maxY);
        }
        
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        
        return new Vector3(randomX, randomY, randomZ);
    }
    
    public static bool IsPointInBounds(Vector3 point, Vector3 center, Vector3 size, bool ignoreY = true)
    {
        float minX = center.x - size.x / 2;
        float maxX = center.x + size.x / 2;
        
        float minZ = center.z - size.z / 2;
        float maxZ = center.z + size.z / 2;
        
        if (ignoreY)
        {
            return (point.x >= minX && point.x <= maxX) &&
                   (point.z >= minZ && point.z <= maxZ);
        }
        else
        {
            float minY = center.y - size.y / 2;
            float maxY = center.y + size.y / 2;
            return (point.x >= minX && point.x <= maxX) &&
                   (point.y >= minY && point.y <= maxY) &&
                   (point.z >= minZ && point.z <= maxZ);
        }
    }
    
    // 将对象 B 的本地坐标转换为相对于对象 A 的本地坐标
    public static Vector3 ConvertLocalToLocal(Transform targetTransform, Transform referenceTransform)
    {
        // 将对象 B 的本地坐标转换为世界坐标
        Vector3 worldPosition = targetTransform.TransformPoint(targetTransform.localPosition);
        
        // 将世界坐标转换为对象 A 的本地坐标
        Vector3 localPositionRelativeToA = referenceTransform.InverseTransformPoint(worldPosition);
        
        return localPositionRelativeToA;
    }
    
    
    public static void MeasureExecutionTime(Action action, string methodName)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        Console.WriteLine($"{methodName} 运行时间: {stopwatch.ElapsedMilliseconds} ms");
    }
}