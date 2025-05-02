using UnityEngine;

public class DireBoar : IEmployeeEnemy
{
    public DireBoar(GameObject obj) : base(obj) { }
    protected override void OnCharacterStart()
    {
        base.OnCharacterStart();
        attackState =typeof(DireBoarRunState);
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
