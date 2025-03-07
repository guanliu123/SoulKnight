
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
        { if(!_buf["levelid"].IsNumber) { throw new SerializationException(); }  Levelid = _buf["levelid"]; }
        { if(!_buf["path"].IsString) { throw new SerializationException(); }  Path = _buf["path"]; }
        { if(!_buf["type"].IsString) { throw new SerializationException(); }  Type = _buf["type"]; }
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
    /// 所属关卡ID
    /// </summary>
    public readonly int Levelid;
    /// <summary>
    /// 房间tilemap加载路径
    /// </summary>
    public readonly string Path;
    /// <summary>
    /// 房间可用作的类型
    /// </summary>
    public readonly string Type;
   
    public const int __ID__ = 2553083;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "id:" + Id + ","
        + "levelid:" + Levelid + ","
        + "path:" + Path + ","
        + "type:" + Type + ","
        + "}";
    }
}

}

