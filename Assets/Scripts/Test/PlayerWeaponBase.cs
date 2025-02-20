using UnityEngine;

public class PlayerWeaponBase:WeaponBase
{
    public new PlayerBase player{get=>base.character as PlayerBase;
        set => base.character = value;
    }
    
    public PlayerWeaponBase(GameObject obj, CharacterBase character) : base(obj, character)
    {
        
    }
}