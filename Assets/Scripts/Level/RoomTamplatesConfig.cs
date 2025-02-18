using System.Collections.Generic;
using EnumCenter;
using UnityEngine;
[System.Serializable]
public class RoomTamplatesConfig
{
    public List<GameObject> BasicRoomTemplates;

    public List<GameObject> BossRoomTemplates;

    public List<GameObject> EnemyRoomTemplates;

    public List<GameObject> EliteEnemyRoomTemplates;

    public List<GameObject> CorridorRoomTemplates;

    public List<GameObject> BirthRoomTemplates;

    public List<GameObject> TeleportRoomTemplates;

    public List<GameObject> HubRoomTemplates;

    public List<GameObject> TreasureRoomTemplates;

    public List<GameObject> ShopRoomTemplates;

    public List<GameObject> SecretRoomTemplates;

    /// <summary>
    /// Get room templates for a given room.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public List<GameObject> GetRoomTemplates(CustomRoom room)
    {
        switch (room.RoomType)
        {
            case RoomType.BossRoom:
                return BossRoomTemplates;

            case RoomType.EnemyRoom:
                return EnemyRoomTemplates;

            // case RoomType.EliteEnemyRoom:
            //     return EliteEnemyRoomTemplates;

            case RoomType.ShopRoom:
                return ShopRoomTemplates;

            case RoomType.TreasureRoom:
                return TreasureRoomTemplates;

            case RoomType.BirthRoom:
                return BirthRoomTemplates;

            case RoomType.TeleportRoom:
                return TeleportRoomTemplates;

            // case RoomType.SecretRoom:
            //     return SecretRoomTemplates;

            default:
                return BasicRoomTemplates;
        }
    }
}
