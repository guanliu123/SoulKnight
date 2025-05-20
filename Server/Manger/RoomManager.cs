using System;
using System.Collections.Generic;
using KnightServer;

public class RoomManager
{
    // 存储房间名称到房间实例的映射
    private Dictionary<string, Room> roomDict;
    
    // 单例模式
    private static RoomManager instance;
    public static RoomManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RoomManager();
            }
            return instance;
        }
    }

    private RoomManager()
    {
        roomDict = new Dictionary<string, Room>();
    }

    // 创建新房间
    public Room CreateRoom(Client client, string roomName, int maxNum = 4)
    {
        if (roomDict.ContainsKey(roomName))
        {
            throw new Exception($"房间 {roomName} 已存在");
        }

        Room newRoom = new Room(roomName,   maxNum,client);
        roomDict.Add(roomName, newRoom);
        return newRoom;
    }

    // 获取房间
    public Room GetRoom(string roomName)
    {
        if (!roomDict.ContainsKey(roomName))
        {
            return null;
        }
        return roomDict[roomName];
    }

    // 删除房间
    public bool RemoveRoom(string roomName)
    {
        if (!roomDict.ContainsKey(roomName))
        {
            return false;
        }
        return roomDict.Remove(roomName);
    }

    // 获取所有房间
    public List<Room> GetAllRooms()
    {
        return new List<Room>(roomDict.Values);
    }

    // 检查房间是否存在
    public bool RoomExists(string roomName)
    {
        return roomDict.ContainsKey(roomName);
    }

    // 获取房间数量
    public int GetRoomCount()
    {
        return roomDict.Count;
    }

    public List<Client> GetRoomPlayers(string roomName){
        if(RoomExists(roomName)){
            return GetRoom(roomName).Clients;
        }
        return null;
    }
}
