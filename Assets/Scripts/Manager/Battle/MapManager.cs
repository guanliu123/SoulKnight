// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using cfg;
// using EnumCenter;
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using Random = UnityEngine.Random;
//
// public struct RoomInfo
// {
//     public Vector3Int centerPoint;
//     public int roomWidth;
//     public int roomHeight;
// }
//
// public class MapManager : MonoSingletonBase<MapManager>
// {
//     private int roomMaxW;
//     private int roomMaxH;
//     //地图中房间生成总数
//     private int mapCount;
//     private int mapMaxW;
//     private int mapMaxH;
//     private int distance;
//     private int corridorWidth;
//     
//     [Header("地板")] public TileBase floor;
//     [Header("墙")] public TileBase wall;
//     [Header("地图")] public Tilemap tilemap;
//     
//     private int[,] _roomMap;
//     private Dictionary<int,RoomInfo> _mapPoint;
//     private Dictionary<int, bool> isConnect=new();
//
//     private int level;
//     private Dictionary<RoomType, List<Room>> _rooms;
//     
//     //private TServerDataLogin=
//
//     public override void Init()
//     {
//         base.Init();
//         InitConfig();
//         InitRooms();
//     }
//
//     public void InitConfig()
//     {
//         var roomRule = ConfigData._Data[ConfigId.RoomMaxSize].Value2.Split("|");
//         roomMaxW = int.Parse(roomRule[0]);
//         roomMaxH = int.Parse(roomRule[1]);
//         mapCount = ConfigData._Data[ConfigId.MapRoomCount].Value1;
//         var t = ConfigData._Data[ConfigId.MapRoomLayout].Value2.Split("|");
//         mapMaxW= int.Parse(t[0]); 
//         mapMaxH= int.Parse(t[1]);
//         distance = ConfigData._Data[ConfigId.DistanceBetweenRoom].Value1;
//     }
//     
//     public void InitRooms()
//     {
//         var roomConfigs = TableManager.Instance.Tables.TbRoom.DataList;
//         if (_rooms==null) _rooms = new();
//         else _rooms.Clear();
//         level = LevelManager.Instance.NowLevel;
//         foreach (var item in roomConfigs)
//         {
//             if (item.Levelbelong == level)
//             {
//                 RoomType type = (RoomType)item.Type;
//                 _rooms.TryAdd(type, new List<Room>());
//                 _rooms[type].Add(item);
//             }
//         }
//     }
//     
//     //画出地图
//     public void GenerateMap()
//     {
//         if (LevelManager.Instance.NowLevel != level)
//         {
//             InitRooms();
//         }
//         
//         tilemap.ClearAllTiles(); // 清空原有地图
//         _roomMap = GetRoomMap();
//         //_centerPoint = new List<Vector3Int>();
//         _mapPoint = new Dictionary<int, RoomInfo>();
//
//         // 遍历所有房间并生成
//         for (int x = 0; x < mapMaxW; x++)
//         {
//             for (int y = 0; y < mapMaxH; y++)
//             {
//                 if (_roomMap[x, y] == 1)
//                 {
//                     // 计算房间坐标偏移（根据间隔距离）
//                     int offsetX = x * (roomMaxW);
//                     int offsetY = y * (roomMaxH);
//
//                     RoomInfo roomInfo;
//                     var t = DrawRoom(offsetX, offsetY);
//                     roomInfo.roomWidth = t.Width;
//                     roomInfo.roomHeight = t.Height;
//                     roomInfo.centerPoint=new Vector3Int(
//                         offsetX, 
//                         offsetY, 
//                         0
//                     );
//                     //_centerPoint.Add(center);
//                     int index = GetIndex(x,y);
//                     _mapPoint[index] = roomInfo;
//                 }
//             }
//         }
//
//         DrawRoad();
//         //DrawFloor(); // 填充地板和墙壁逻辑
//         EventManager.Instance.Emit(EventId.MAP_GENERATION_COMPLETED);
//     }
//     
//     //二维数组坐标转换为一个index
//     int GetIndex(int row, int col) => row * _roomMap.GetLength(1) + col;
//     //从一个index获取二维数组坐标
//     (int row, int col) GetRowCol(int index)=> (index / _roomMap.GetLength(1), index % _roomMap.GetLength(1));
// //画出房间
//     private Room DrawRoom(int roomX, int roomY)
//     {
//         var room = RandomRoom();
//         var asset = LoadManager.Instance.Load<GameObject>
//             ("Prefabs/Room/"+room.Path+".prefab");
//         GameObject roomObj = LoadManager.Instance.Instantiate(asset) as GameObject;
//         if(roomObj) roomObj.transform.position = new Vector3(roomX,roomY);
//
//         return room;
//     }
// //画出路
//     private void DrawRoad()
//     {
//         foreach (var item in _mapPoint)
//         {
//             int key = item.Key;
//             var t = GetRowCol(key);
//             int x=t.row,y=t.col;
//
//             if (CheckConnect(x - 1, y))
//             {
//                 ConnectRooms(key,GetIndex(x-1,y));
//             }
//             if (CheckConnect(x + 1, y))
//             {
//                 ConnectRooms(key,GetIndex(x+1,y));
//             }
//             if (CheckConnect(x, y-1))
//             {
//                 ConnectRooms(key,GetIndex(x,y-1));
//             }
//             if (CheckConnect(x, y+1))
//             {
//                 ConnectRooms(key,GetIndex(x,y+1));
//             }
//             
//             isConnect.TryAdd(key, true);
//         }
//     }
//
//     private bool CheckConnect(int x,int y)
//     {
//         if (x < 0 || x >= _roomMap.GetLength(0) || y < 0 || y > _roomMap.GetLength(1)) return false;
//         int index = GetIndex(x, y);
//         return _mapPoint.ContainsKey(index) && !isConnect.ContainsKey(index);
//     }
//     
//     // 房间连接算法（A*简化版）
//     private void ConnectRooms(int startIdx, int endIdx)
//     {
//         RoomInfo startRoom = _mapPoint[startIdx];
//         Vector3Int start = startRoom.centerPoint;
//         RoomInfo endRoom = _mapPoint[endIdx];
//         Vector3Int end = endRoom.centerPoint;
//         
//         Vector3Int dir = new Vector3Int(Math.Sign(end.x - start.x),Math.Sign(end.y - start.y));
//         //从需要连通的墙壁为起点和终点
//         start = new Vector3Int(start.x + dir.x * (int)(0.5 * (startRoom.roomWidth + 1)),
//             start.y+dir.y*(int)(0.5*(startRoom.roomHeight+1)));
//         end = new Vector3Int(end.x-dir.x*(int)(0.5*(endRoom.roomWidth+1)),
//             end.y-dir.y*(int)(0.5*(endRoom.roomHeight+1)));
//         //确认走廊长度
//         int len = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y)+distance;
//         for (int i = 0; i < len; i++)
//         {
//             for (int j = 0; j < corridorWidth; j++)
//             {
//                 //说明是横向走廊，竖向画一列，第1和corridorWidth格是墙壁
//                 if (dir.x != 0)
//                 {
//                     var pos = new Vector3Int(start.x + i * dir.x, start.y + (j - corridorWidth / 2));
//                     //墙壁
//                     if (j == 0 || j == corridorWidth - 1)
//                     {
//                         tilemap.SetTile(pos, wall);
//                         continue;
//                     }
//                     tilemap.SetTile(pos, floor);
//                 }
//                 else
//                 {
//                     var pos = new Vector3Int(start.x + (j - corridorWidth / 2), start.y + i * dir.y);
//                     if (j == 0 || j == corridorWidth - 1)
//                     {
//                         tilemap.SetTile(pos, wall);
//                         continue;
//                     }
//                     tilemap.SetTile(pos, floor);
//                 }
//             }
//         }
//     }
// //画出地板和墙壁
// private void DrawFloor(){BoundsInt bounds = tilemap.cellBounds;
//     
//     foreach (var pos in bounds.allPositionsWithin)
//     {
//         if (tilemap.GetTile(pos) == null)
//         {
//             // 查找周围是否有关联房间
//             if (CheckNearbyFloor(pos))
//             {
//                 tilemap.SetTile(pos, floor);
//                 GenerateWallAround(pos);
//             }
//         }
//     }}
// private bool CheckNearbyFloor(Vector3Int pos)
// {
//     for (int x = -1; x <= 1; x++)
//     {
//         for (int y = -1; y <= 1; y++)
//         {
//             if (tilemap.GetTile(pos + new Vector3Int(x, y, 0)) != null)
//                 return true;
//         }
//     }
//     return false;
// }
// private void GenerateWallAround(Vector3Int pos)
// {
//     for (int x = -1; x <= 1; x++)
//     {
//         for (int y = -1; y <= 1; y++)
//         {
//             Vector3Int wallPos = pos + new Vector3Int(x, y, 0);
//             if (tilemap.GetTile(wallPos) == null)
//             {
//                 tilemap.SetTile(wallPos, wall);
//             }
//         }
//     }
// }
//
// // 生成路径地板（带墙壁保护）
// private void SetFloorWithWall(Vector3Int pos)
// {
//     // 核心路径
//     tilemap.SetTile(pos, floor);
//     
//     // 两侧墙壁保护
//     tilemap.SetTile(pos + Vector3Int.up, wall);
//     tilemap.SetTile(pos + Vector3Int.down, wall);
//     tilemap.SetTile(pos + Vector3Int.left, wall);
//     tilemap.SetTile(pos + Vector3Int.right, wall);
// }
// //生成房间 （用二维 int 数组表示）
//     private Room RandomRoom()
//     {
//         int index = Random.Range(0,_rooms[RoomType.EnemyRoom].Count);
//         return _rooms[RoomType.EnemyRoom][index];
//         // int width = GetOddNumber(roomMinW, roomMaxW);
//         // int height = GetOddNumber(roomMinH, roomMaxH);   
//         // var room = new int[width, height];
//         // //方便以后扩展使用了二维数组，这里为了演示方便对房间内生成其他物体
//         // for (var i = 0; i < width; i++)
//         // {
//         //     for (var j = 0; j < height; j++)
//         //     {
//         //         room[i, j] = 1;
//         //     }
//         // }
//         // return room;
//     }
// //生成一个地图 （用二维 int 数组表示）
//     private int[,] GetRoomMap()
//     {
//         //第一个房间的坐标点
//         var nowPoint = Vector2Int.zero;
//         //当前生成的房间数
//         var mCount = 1;
//         //当前地图
//         var map = new int[mapMaxW, mapMaxH];
//         //第一个格子总有房间，作为出生房间
//         map[nowPoint.x, nowPoint.y] = 1;
//     
//         while (mCount < mapCount)
//         {
//             nowPoint = GetNextPoint(nowPoint, mapMaxW, mapMaxH);
//             if (map[nowPoint.x, nowPoint.y] == 1) continue;
//             map[nowPoint.x, nowPoint.y] = 1;
//             mCount ++;
//         }
//         return map;
//     }
//     
// //获取下一个房间的位置
//     private Vector2Int GetNextPoint(Vector2Int nowPoint, int maxW, int maxH)
//     {
//         while (true)
//         {
//             var mNowPoint = nowPoint;
//
//             switch (Random.Range(0, 4))
//             {
//                 case 0:
//                     mNowPoint.x += 1;
//                     break;
//                 case 1:
//                     mNowPoint.y += 1;
//                     break;
//                 case 2:
//                     mNowPoint.x -= 1;
//                     break;
//                 default:
//                     mNowPoint.y -= 1;
//                     break;
//             }
//
//             if (mNowPoint.x >= 0 && mNowPoint.y >= 0 && mNowPoint.x < maxW && mNowPoint.y < maxH)
//             {
//                 return mNowPoint;
//             }
//         }
//     }
// }

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using cfg;
using Cysharp.Threading.Tasks;
using Edgar.Unity;
using EnumCenter;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapManager : MonoSingletonBase<MapManager>
{
    private DungeonGeneratorGrid2D dungeonGenerator;
    private IRoomConfig roomConfig;

    private bool isInit;
    private int level;
    private int stage;
    
    public override void Init()
    {
        base.Init();
         InitGenerator().Forget();
    }

    private async UniTask InitGenerator()
    {
        try
        {
            if (roomConfig == null || level != LevelManager.Instance.NowLevelNumber)
            {
                await UpdateRoomConfig();
            }
            dungeonGenerator = gameObject.AddComponent<DungeonGeneratorGrid2D>();
            dungeonGenerator.GeneratorConfig = new();
            dungeonGenerator.PostProcessConfig = new();
            gameObject.AddComponent<RoomPostProcessing>();
        
            dungeonGenerator.InputType=DungeonGeneratorInputTypeGrid2D.CustomInput;
            var gungeonCustomInput = ScriptableObject.CreateInstance<GungeonCustomInput>();
            gungeonCustomInput.roomConfig = roomConfig;
            dungeonGenerator.CustomInputTask = gungeonCustomInput;
            
            EventManager.Instance.Emit(EventId.MAPMANAGER_CONFIG_UPDATE_COMPLETED);
            isInit = true;
        }
        catch (Exception e)
        {
            LogTool.LogError(e);
            throw;
        }
    }

    private async UniTask UpdateRoomConfig()
    {
        roomConfig = ScriptableObject.CreateInstance<IRoomConfig>();
        roomConfig.levelGraphs = new();
        
        var _levelConfig = TableManager.Instance.Tables.TBLevel;

        //加载布局
        var _levelGraphList = _levelConfig[LevelManager.Instance.NowLevelID].LevelGraphPath.Split("|");
        foreach (var item in _levelGraphList)
        {
            var temp = LoadGraph(item);
            if (roomConfig.LevelGraph == null)
            {
                roomConfig.LevelGraph = temp;
            }
            roomConfig.levelGraphs.Add(temp);
        }
        roomConfig.LevelGraphBoss =LoadGraph(_levelConfig[LevelManager.Instance.NowLevelID].LevelBossGraphPath);
        
        //设置一些参数
        roomConfig.UseRandomLevelGraph = true;
        roomConfig.ExtraEnemyRoomChance = 0.9f;
        roomConfig.ExtraEnemyRoomDeadEndChance = 0.1f;
        roomConfig.TreasureRoomDeadEndChance = 0.5f;
        roomConfig.SecretRoomChance = 0;
        roomConfig.SecretRoomDeadEndChance = 0;
        
        //加载房间模板
        RoomTamplatesConfig _tamplate = new RoomTamplatesConfig();

        var roomList = TableManager.Instance.Tables.TbRoom.DataList;
        foreach (var item in roomList)
        {
            if (item.Levelid == LevelManager.Instance.NowLevelID)
            {
                GameObject room = LoadManager.Instance.Load<GameObject>("Prefabs/Room/Forest/"+item.Path+".prefab");
                
                var types = item.Type.Split("|");
                for (int i = 0; i < types.Length; i++)
                {
                    var type = (RoomType)int.Parse(types[i]);

                    switch (type)
                    {
                        case RoomType.BasicRoom:
                        {
                            if (_tamplate.BasicRoomTemplates == null) _tamplate.BasicRoomTemplates = new();
                            _tamplate.BasicRoomTemplates.Add(room);
                        }break;
                        case RoomType.BirthRoom: {
                            if (_tamplate.BirthRoomTemplates == null) _tamplate.BirthRoomTemplates = new();
                            _tamplate.BirthRoomTemplates.Add(room);
                        }break;
                        case RoomType.EnemyRoom: {
                            if (_tamplate.EnemyRoomTemplates == null) _tamplate.EnemyRoomTemplates = new();
                            _tamplate.EnemyRoomTemplates.Add(room);
                        }break;
                        case RoomType.BossRoom: {
                            if (_tamplate.BossRoomTemplates == null) _tamplate.BossRoomTemplates = new();
                            _tamplate.BossRoomTemplates.Add(room);
                        }break;
                        case RoomType.TeleportRoom: {
                            if (_tamplate.TeleportRoomTemplates == null) _tamplate.TeleportRoomTemplates = new();
                            _tamplate.TeleportRoomTemplates.Add(room);
                        }break;
                        case RoomType.TreasureRoom: {
                            if (_tamplate.TreasureRoomTemplates == null) _tamplate.TreasureRoomTemplates = new();
                            _tamplate.TreasureRoomTemplates.Add(room);
                        }break;
                        case RoomType.ShopRoom: {
                            if (_tamplate.ShopRoomTemplates == null) _tamplate.ShopRoomTemplates = new();
                            _tamplate.ShopRoomTemplates.Add(room);
                        }break;
                        case RoomType.Corridor: {
                            if (_tamplate.CorridorRoomTemplates == null) _tamplate.CorridorRoomTemplates = new();
                            _tamplate.CorridorRoomTemplates.Add(room);
                        }break;
                            default: break;
                    }
                }
            }
        }
        roomConfig.RoomTemplates = _tamplate;
        
        //如果mapmanager已经初始化过了，但是可能因为大关的变动导致要重新载入roomconfig，所以由这个函数触发事件
        //因为在InitGenerator是先调用这个函数再赋值给dungeonGenerator，这个函数不可以先发出事件
        if(isInit) EventManager.Instance.Emit(EventId.MAPMANAGER_CONFIG_UPDATE_COMPLETED);
    }

    public void GenerateMap()
    {
        if (dungeonGenerator == null)
        {
            LogTool.LogError("地图生成器未初始化！请检查是否使用了Init!");
            return;
        }

        dungeonGenerator.Generate();
        EventManager.Instance.Emit(EventId.MAP_GENERATION_COMPLETED);
    }

    private LevelGraph LoadGraph(string path)
    {
        LevelGraph res = null;
        
        var _asset = LoadManager.Instance.Load<ScriptableObject>("Prefabs/Room/LevelGraph/"+path+".asset");
        if (_asset != null)
        {
            res = _asset as LevelGraph;
        }

        return res;
    }
}