using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [Header("地图种子")] public int seed = 123456;
    [Header("每行房间数")] public int mapMaxW;
    [Header("每列房间数")] public int mapMaxH;
    [Header("总生成房间数")] public int mapCount;
    [Header("最大房间宽")] public int roomMaxW;
    [Header("最大房间高")] public int roomMaxH;
    [Header("最小房间宽")] public int roomMinW;
    [Header("最小房间高")] public int roomMinH;
    [Header("房间的间隔距离")] public int distance;
    [Header("地板")] public TileBase floor;
    [Header("墙")] public TileBase wall;
    [Header("地图")] public Tilemap tilemap;
    private int[,] _roomMap;
    private List<Vector3Int> _centerPoint;
    private Dictionary<Vector3Int,int> _mapPoint;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DrawMap();
        }
    }
    
    //画出地图
    private void DrawMap()
    {
        tilemap.ClearAllTiles(); // 清空原有地图
        _roomMap = GetRoomMap();
        _centerPoint = new List<Vector3Int>();
        _mapPoint = new Dictionary<Vector3Int, int>();

        // 遍历所有房间并生成
        for (int x = 0; x < mapMaxW; x++)
        {
            for (int y = 0; y < mapMaxH; y++)
            {
                if (_roomMap[x, y] == 1)
                {
                    // 计算房间坐标偏移（根据间隔距离）
                    int offsetX = x * (roomMaxW + distance);
                    int offsetY = y * (roomMaxH + distance);
                
                    DrawRoom(offsetX, offsetY);
                    var center = new Vector3Int(
                        offsetX + roomMaxW/2, 
                        offsetY + roomMaxH/2, 
                        0
                    );
                    _centerPoint.Add(center);
                    _mapPoint[center] = 1;
                }
            }
        }

        DrawRoad();
        DrawFloor(); // 填充地板和墙壁逻辑
    }
//画出房间
    private void DrawRoom(int roomX, int roomY)
    {
        var room = RandomRoom();
        int w = room.GetLength(0);
        int h = room.GetLength(1);

        // 生成房间边界和内部
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3Int pos = new Vector3Int(
                    roomX + x - w/2,  // 中心对齐
                    roomY + y - h/2, 
                    0
                );
            
                // 边界为墙壁
                if (x == 0 || y == 0 || x == w-1 || y == h-1)
                {
                    tilemap.SetTile(pos, wall);
                }
                else  // 内部为地板
                {
                    tilemap.SetTile(pos, floor);
                }
            }
        }
    }
//画出路
    private void DrawRoad()
    {
        // 根据生成顺序连接房间中心
        for (int i = 1; i < _centerPoint.Count; i++)
        {
            var start = _centerPoint[i - 1];
            var end = _centerPoint[i];
            ConnectRooms(start, end);
        }
    }
    // 房间连接算法（A*简化版）
    private void ConnectRooms(Vector3Int start, Vector3Int end)
    {
        // 先横向再纵向连接
        int dirX = (int)Mathf.Sign(end.x - start.x);
        for (int x = start.x; x != end.x; x += dirX)
        {
            SetFloorWithWall(new Vector3Int(x, start.y, 0));
        }

        int dirY = (int)Mathf.Sign(end.y - start.y);
        for (int y = start.y; y != end.y; y += dirY)
        {
            SetFloorWithWall(new Vector3Int(end.x, y, 0));
        }
    }
//画出地板和墙壁
private void DrawFloor(){BoundsInt bounds = tilemap.cellBounds;
    
    foreach (var pos in bounds.allPositionsWithin)
    {
        if (tilemap.GetTile(pos) == null)
        {
            // 查找周围是否有关联房间
            if (CheckNearbyFloor(pos))
            {
                tilemap.SetTile(pos, floor);
                GenerateWallAround(pos);
            }
        }
    }}
private bool CheckNearbyFloor(Vector3Int pos)
{
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            if (tilemap.GetTile(pos + new Vector3Int(x, y, 0)) != null)
                return true;
        }
    }
    return false;
}
private void GenerateWallAround(Vector3Int pos)
{
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            Vector3Int wallPos = pos + new Vector3Int(x, y, 0);
            if (tilemap.GetTile(wallPos) == null)
            {
                tilemap.SetTile(wallPos, wall);
            }
        }
    }
}

// 生成路径地板（带墙壁保护）
private void SetFloorWithWall(Vector3Int pos)
{
    // 核心路径
    tilemap.SetTile(pos, floor);
    
    // 两侧墙壁保护
    tilemap.SetTile(pos + Vector3Int.up, wall);
    tilemap.SetTile(pos + Vector3Int.down, wall);
    tilemap.SetTile(pos + Vector3Int.left, wall);
    tilemap.SetTile(pos + Vector3Int.right, wall);
}
//生成房间 （用二维 int 数组表示）
    private int[,] RandomRoom()
    {
        int width = GetOddNumber(roomMinW, roomMaxW);
        int height = GetOddNumber(roomMinH, roomMaxH);   
        var room = new int[width, height];
        //方便以后扩展使用了二维数组，这里为了演示方便对房间内生成其他物体
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                room[i, j] = 1;
            }
        }
        return room;
    }
//生成一个地图 （用二维 int 数组表示）
    private int[,] GetRoomMap()
    {
        //第一个房间的坐标点
        var nowPoint = Vector2Int.zero;
        //当前生成的房间数
        var mCount = 1;
        //当前地图
        var map = new int[mapMaxW, mapMaxH];
        //第一个格子总有房间，作为出生房间
        map[nowPoint.x, nowPoint.y] = 1;
    
        while (mCount < mapCount)
        {
            nowPoint = GetNextPoint(nowPoint, mapMaxW, mapMaxH);
            if (map[nowPoint.x, nowPoint.y] == 1) continue;
            map[nowPoint.x, nowPoint.y] = 1;
            mCount ++;
        }
        return map;
    }

//获取一个范围内的随机奇数
private int GetOddNumber(int min, int max){
    while (true)
{
    var temp = Random.Range(min, max);
    if ((temp & 1) != 1) continue;
    return temp;
}
}
//获取下一个房间的位置
    private Vector2Int GetNextPoint(Vector2Int nowPoint, int maxW, int maxH)
    {
        while (true)
        {
            var mNowPoint = nowPoint;

            switch (Random.Range(0, 4))
            {
                case 0:
                    mNowPoint.x += 1;
                    break;
                case 1:
                    mNowPoint.y += 1;
                    break;
                case 2:
                    mNowPoint.x -= 1;
                    break;
                default:
                    mNowPoint.y -= 1;
                    break;
            }

            if (mNowPoint.x >= 0 && mNowPoint.y >= 0 && mNowPoint.x < maxW && mNowPoint.y < maxH)
            {
                return mNowPoint;
            }
        }
    }
}
