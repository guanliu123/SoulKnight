using UnityEngine;

public class Boar : IEmployeeEnemy
{
    public Boar(GameObject obj) : base(obj) { }
    protected override void OnCharacterStart()
    {
        base.OnCharacterStart();
        attackState = typeof(BoarAttackState);
        m_StateController.SetOtherState(typeof(EnemyTrackState));
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
