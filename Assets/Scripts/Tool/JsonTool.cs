#region

using Newtonsoft.Json;

#endregion

public class JsonTool
{
    public static string SerializeObject(object obj)
    {
        JsonSerializerSettings settings = new();
        settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        string text = JsonConvert.SerializeObject(obj, settings);
        return text;
    }
#nullable enable
    public static T? DeserializeObject<T>(string value)
    {
        JsonSerializerSettings settings = new();
        settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        return JsonConvert.DeserializeObject<T>(value, settings);
    }
    
    public static object? DeserializeObject(string value)
    {
        JsonSerializerSettings settings = new();
        settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        return JsonConvert.DeserializeObject<object>(value, settings);
    }
}