using UnityEngine;

public class AK47:PlayerWeaponBase
{
    public AK47(GameObject obj, CharacterBase character) : base(obj, character)
    {
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