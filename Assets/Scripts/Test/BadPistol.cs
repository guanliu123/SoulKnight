using UnityEngine;

public class BadPistol:PlayerWeaponBase
{
    public BadPistol(GameObject obj, CharacterBase character) : base(obj, character)
    {
    }

    protected override void OnFire()
    {
        base.OnFire();
    }
}