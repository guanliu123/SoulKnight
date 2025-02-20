using UnityEngine;

public class BadPistol:PlayerWeaponBase
{
    public BadPistol(GameObject obj, CharacterBase character) : base(obj, character)
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void OnFire()
    {
        base.OnFire();
    }
    public override void OnEnter()
    {
        base.OnEnter();
        gameObject.SetActive(true);
    }

    public override void OnExit()
    {
        base.OnExit();
        gameObject.SetActive(false);
    }
}