using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTrackState : EnemyState
{
    private BoidsCalculator boids = new BoidsCalculator();
    private List<IEmployeeEnemy> allEnemies; // 需从管理器获取
    private QuadTreeSystem quadTreeSystem;
     private AvoidanceCalculator avoidanceCalculator;
     private IEmployeeEnemy enemy;
     private bool isFirstEnter;
     
    public EnemyTrackState(EnemyStateController machine) : base(machine)
    {
        enemy = m_Controller.GetCharacter();
    }

    protected override void StateStart()
    {
        base.StateStart();
        allEnemies = GameMediator.Instance.GetController<EnemyController>().enemysInRoom;
        quadTreeSystem = GameMediator.Instance.GetSystem<QuadTreeSystem>();
        avoidanceCalculator = new AvoidanceCalculator(quadTreeSystem.QuadTree);
        m_Animator.SetBool("isIdle", false);
        if (!isFirstEnter)
        {
            isFirstEnter = true;
            m_Controller.SetOtherState(enemy.attackState);
        }
        else
        {
            CoroutinePool.Instance.StartCoroutine(AttackPlayer(), this);
        }
        
        EventCenter.Instance.NotisfyObserver(EventType.OnEnemyBeginTrackPlayer,enemy);
    }

    public override void GameUpdate()
    {
        base.GameUpdate();
        GetBoidsMove();
    }

    private void GetBoidsMove()
    {
        // 获取所有敌人实例（建议通过管理器缓存）
        //var boidsDir = boids.CalculateBoidsMove(enemy, allEnemies);
        
        // 混合原始输入和Boids方向
        //Vector2 finalDir = Vector2.Lerp(enemy.Velocity.ToVector2(), boidsDir, 0.7f);
        var finalDir = UpdateMovement();
        if (finalDir.magnitude > 0)
        {
            enemy.transform.position += (Vector3)finalDir * 
                                        enemy.m_Attr.m_ShareAttr.Speed * Time.deltaTime;
            
            enemy.isLeft = finalDir.x < 0;
        }
    }
    
    protected Vector2 UpdateMovement()
    {
        Vector2 baseDir = boids.CalculateBoidsMove(enemy, allEnemies);
        Vector2 avoidDir = CalculateAvoidance();

        var att = enemy.m_Attr;
        //避障时的速度补偿
        float aspeed = 1.5f;

        // 方向合成（保留原始速度的90%）
        Vector2 finalDir = Vector2.Lerp(baseDir, avoidDir*aspeed , 0.2f);
        return finalDir;
    }

    private Vector2 CalculateAvoidance()
    {
        // 使用优化后的四叉树查询
        var obstacles = quadTreeSystem.QueryCircle(
            enemy.transform.position, 
              3f
        ).OfType<RectCollider>().Where(c => c.IsObstacle).ToList();

        return avoidanceCalculator.Calculate(enemy.rectCollider, obstacles);
    }

    protected override void StateEnd()
    {
        base.StateEnd();
        EventCenter.Instance.NotisfyObserver(EventType.OnEnemyStopTrackPlayer,enemy);
    }
    
    private IEnumerator AttackPlayer()
    {
        yield return new WaitForSeconds(5);
        m_Controller.SetOtherState(enemy.attackState);
    }
}