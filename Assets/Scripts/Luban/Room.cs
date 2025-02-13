
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg
{
public sealed partial class Room : Luban.BeanBase
{
    public Room(JSONNode _buf) 
    {
        { if(!_buf["id"].IsNumber) { throw new SerializationException(); }  Id = _buf["id"]; }
        { if(!_buf["path"].IsString) { throw new SerializationException(); }  Path = _buf["path"]; }
        { if(!_buf["type"].IsNumber) { throw new SerializationException(); }  Type = _buf["type"]; }
        { if(!_buf["width"].IsNumber) { throw new SerializationException(); }  Width = _buf["width"]; }
        { if(!_buf["Length"].IsNumber) { throw new SerializationException(); }  Length = _buf["Length"]; }
    }

    public static Room DeserializeRoom(JSONNode _buf)
    {
        return new Room(_buf);
    }

    /// <summary>
    /// ID
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 房间tilemap加载路径
    /// </summary>
    public readonly string Path;
    /// <summary>
    /// 类型
    /// </summary>
    public readonly int Type;
    /// <summary>
    /// 房间宽度
    /// </summary>
    public readonly int Width;
    /// <summary>
    /// 房间长度
    /// </summary>
    public readonly int Length;
   
    public const int __ID__ = 2553083;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "id:" + Id + ","
        + "path:" + Path + ","
        + "type:" + Type + ","
        + "width:" + Width + ","
        + "Length:" + Length + ","
        + "}";
    }
}

}

