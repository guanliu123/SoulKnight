using UnityEngine;

public class DoubleBladeSword:PlayerWeaponBase
{
    public DoubleBladeSword(GameObject obj, CharacterBase character) : base(obj, character)
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