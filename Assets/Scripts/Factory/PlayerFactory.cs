using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class PlayerFactory : SingletonBase<PlayerFactory>
{
    public PlayerBase GetPlayer(PlayerType type)
    {
        PlayerBase player = null;

        GameObject obj = GameObject.Find(type.ToString());
        switch (type)
        {
            case PlayerType.Knight: player = new Knight(obj);
                break;
        }

        return player;
    }
}
