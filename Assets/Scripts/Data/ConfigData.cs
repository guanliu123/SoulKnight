using System.Collections;
using System.Collections.Generic;
using cfg;
using UnityEngine;

public static class ConfigData
{
    public static TBConfigData _Data { get; private set; }

    public static void Init()
    {
        _Data = TableManager.Instance.Tables.TBConfigData;
    }
}
