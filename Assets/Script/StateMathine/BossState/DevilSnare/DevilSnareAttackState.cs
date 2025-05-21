using System.Collections.Generic;
using UnityEngine;
public class DevilSnareAttackState : BossState
{
    private List<IBossSkill> skills;
    public DevilSnareAttackState(BossStateController controller) : base(controller) { }
    protected override void StateInit()
    {
        base.StateInit();
        skills = boss.skills;
    }
    protected override void StateStart()
    {
        base.StateStart();
        m_Animator.SetTrigger("isAttack");
        int idx = RandomTool.GetEnemyRandomInt(0, skills.Count);
        Debug.Log("Boss当前招式idx:"+idx);
        skills[idx].StartSkill();
        m_Controller.SetOtherState(typeof(DevilSnareIdleState));
    }
}
