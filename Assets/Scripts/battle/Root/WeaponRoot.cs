using System.Collections;
using System.Collections.Generic;
using EnumCenter;
using UnityEngine;

public class WeaponRoot : MonoBehaviour
{
    public PlayerWeaponType weaponType;
    
    public Transform firePoint;
    
    public Transform GetAnimator()
    {
        if (firePoint == null)
        {
            LogTool.LogError($"武器{firePoint.ToString()}上的FirePoint组件未赋值！");
        }

        return firePoint;
    }
}
