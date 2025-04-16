using UnityEngine;
using System.Collections.Generic;
using OPH.Collision.QuadTree;

public class AvoidanceCalculator
{
    // 配置参数
    public float maxAvoidForce = 1.0f;
    public float obstacleDetectionRadius = 2.0f;
    public float edgeBuffer = 0.2f;

    // 四叉树引用
    private QTree<IRect> _quadTree;

    public AvoidanceCalculator(QTree<IRect> quadTree)
    {
        _quadTree = quadTree;
    }

    public Vector2 Calculate(IRect self, List<RectCollider> allObstacles)
    {
        // 步骤1：通过四叉树快速获取附近潜在障碍物
        List<IRect> nearbyObstacles = new List<IRect>();
        _quadTree.GetAroundObj(self, nearbyObstacles);

        Vector2 avoidance = Vector2.zero;
        Vector2 selfPos = new Vector2(self.X, self.Y);

        // 步骤2：精确检测每个障碍物
        foreach (var obstacle in nearbyObstacles)
        {
            if (obstacle == self) continue;

            Vector2 obstaclePos = new Vector2(obstacle.X, obstacle.Y);
            Vector2 toObstacle = obstaclePos - selfPos;
            float distance = toObstacle.magnitude;

            // 计算有效避让距离
            float combinedRadius = (self.Width + obstacle.Width)/2 + edgeBuffer;
            if (distance > combinedRadius) continue;

            // 步骤3：计算排斥方向（基于穿透深度）
            Vector2 dir = toObstacle.normalized;
            float penetration = combinedRadius - distance;
            avoidance += -dir * Mathf.Clamp(penetration * 2, 0, maxAvoidForce);
        }

        return avoidance;
    }

    // 可视化调试方法
    public void DebugDraw(IRect self)
    {
#if UNITY_EDITOR
        Vector2 selfPos = new Vector2(self.X, self.Y);
        
        // 绘制检测范围
        UnityEditor.Handles.color = new Color(1,0,0,0.1f);
        UnityEditor.Handles.DrawWireDisc(selfPos, Vector3.forward, 
            (self.Width/2) + obstacleDetectionRadius);

        // 绘制避让向量
        Vector2 avoidance = Calculate(self, new List<RectCollider>());
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawLine(selfPos, selfPos + avoidance.normalized * 2);
#endif
    }
}