using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class PlayerFactory : SingletonBase<PlayerFactory>
{
    public IPlayer GetPlayer(PlayerType type)
    {
        IPlayer player = null;

        GameObject obj = GameObject.Find(type.ToString());
        switch (type)
        {
            case PlayerType.Knight: player = new Knight(obj);
                break;
        }

        return player;
    }
}
