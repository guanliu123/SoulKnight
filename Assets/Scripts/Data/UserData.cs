using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserData
{
    public static bool isNew { get; private set; }

    public static void UpdateUserData()
    {
        //todo:接收服务器传来的数据更新玩家数据
        isNew = true;
    }
}
