using EnumCenter;
using UnityEngine;

public class WeaponFactory:SingletonBase<WeaponFactory>
{
    /// <summary>
    /// 通过Type生成WeaponBase
    /// </summary>
    /// <param name="type">枪支类型</param>
    /// <param name="character">持有角色</param>
    /// <returns></returns>
    public PlayerWeaponBase GetPlayerWeapon(PlayerWeaponType type, CharacterBase character)
    {
        Transform origin = character.characterRoot.GetWeaponOriginPoint();
        GameObject obj = GameObject.Instantiate(GetWeaponObj(type.ToString()),origin);
        obj.name = type.ToString();
        obj.transform.localPosition=Vector3.zero;

        PlayerWeaponBase weapon=null;
        switch (type)
        {
            case PlayerWeaponType.BadPistol:
                weapon=new BadPistol(obj,character);break;
        }

        return weapon;
    }

    /// <summary>
    /// 从场景中捡到枪支，为枪支生成WeaponBase脚本
    /// </summary>
    /// <param name="weapon">枪支物体</param>
    /// <param name="character">持有角色</param>
    /// <returns></returns>
    public PlayerWeaponBase GetPlayerWeapon(GameObject weaponObj,CharacterBase character)
    {
        WeaponRoot root = weaponObj.GetComponent<WeaponRoot>();
        if (!root)
        {
            LogTool.LogError("场景中捡到的枪支未挂载WeaponRoot！");
            return null;
        }
        
        Transform origin = character.characterRoot.GetWeaponOriginPoint();
        weaponObj.transform.SetParent(origin);
        weaponObj.transform.localPosition=Vector3.zero;
        
        PlayerWeaponBase weapon=null;
        switch (root.weaponType)
        {
            case PlayerWeaponType.BadPistol:
                weapon=new BadPistol(weaponObj,character);break;
        }

        return weapon;
    }
    
    public GameObject GetWeaponObj(string name)
    {
        return LoadManager.Instance.Load<GameObject>("Prefabs/Weapon/" + name + ".prefab");
    }
}