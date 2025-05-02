using UnityEngine;

public class EliteGoblinGuard : IEmployeeEnemy
{
    public EliteGoblinGuard(GameObject obj) : base(obj) { }
    protected override void OnCharacterStart()
    {
        base.OnCharacterStart();

        if (m_Weapon.m_Attr.Type != EnemyWeaponType.Hoe)
        {
            attackState =typeof(EliteGoblinGuardAttackState);
            m_StateController.SetOtherState(typeof(EnemyTrackState));
        }
        else
        {
            attackState =typeof(EliteGoblinGuardMeleeAttackState);
            m_StateController.SetOtherState(typeof(EnemyTrackState));
        }
    }
    protected override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();
    }
    protected override void OnCharacterDieStart()
    {
        base.OnCharacterDieStart();
    }
}
