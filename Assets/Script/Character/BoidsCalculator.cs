using System.Collections.Generic;
using System.Linq;
using OPH.Collision.QuadTree;
using UnityEngine;

public class BoidsCalculator
{
    // 配置参数，参考[kevinwei00的方案](https://github.com/kevinwei00/boids-flocking-simulation)
    public float separationWeight = 1.6f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float perceptionRadius = 2.0f;

    public Vector2 CalculateBoidsMove(IEmployeeEnemy currentEnemy, IEnumerable<IEmployeeEnemy> allEnemies)
    {
        var neighbors = GetNeighbors(currentEnemy, allEnemies);
        
        Vector2 separation = CalculateSeparation(currentEnemy, neighbors);
        Vector2 alignment = CalculateAlignment(currentEnemy, neighbors);
        Vector2 cohesion = CalculateCohesion(currentEnemy, neighbors);

        return (separation * separationWeight + 
                alignment * alignmentWeight + 
                cohesion * cohesionWeight).normalized;
    }

    List<IEmployeeEnemy> GetNeighbors(IEmployeeEnemy current, IEnumerable<IEmployeeEnemy> all)
    {
        // 使用空间分区优化，参考[chenjd的GPU方案](https://github.com/chenjd/Unity-Boids-Behavior-on-GPGPU)
        return all.Where(e => e != current && 
                              Vector2.Distance(e.transform.position, current.transform.position) <= perceptionRadius).ToList();
    }

    Vector2 CalculateSeparation(IEmployeeEnemy current, List<IEmployeeEnemy> neighbors)
    {
        if (!neighbors.Any()) return Vector2.zero;
        
        var relativePositions = neighbors.Select(e => 
            (new FixVector2(e.transform.position) - new FixVector2(current.transform.position))
        );
        
        FixVector2 averagePos = relativePositions.Average();
    
        // 反转方向（Separation需远离）
        return -averagePos.ToVector2();
    }

    Vector2 CalculateAlignment(IEmployeeEnemy current, List<IEmployeeEnemy> neighbors)
    {
        
        if (!neighbors.Any()) return current.Velocity.ToVector2();
        
        var relativePositions = neighbors.Select(e => e.Velocity);
        
        FixVector2 averagePos = relativePositions.Average();
        
        return averagePos.ToVector2();
    }

    Vector2 CalculateCohesion(IEmployeeEnemy current, List<IEmployeeEnemy> neighbors)
    {
        if (!neighbors.Any()) return Vector2.zero;
        
        var relativePositions = neighbors.Select(e => 
            (new FixVector2(e.transform.position) - new FixVector2(current.transform.position))
        );
        
        FixVector2 averagePos = relativePositions.Average();
        
        return averagePos.ToVector2();
    }
}
public static class FixVector2Extensions
{
    public static FixVector2 Average(this IEnumerable<FixVector2> vectors)
    {
        if (!vectors.Any()) return FixVector2.Zero;

        Fix64 sumX = Fix64.Zero;
        Fix64 sumY = Fix64.Zero;
        int count = 0;

        foreach (var vec in vectors)
        {
            sumX += vec.x;
            sumY += vec.y;
            count++;
        }

        return new FixVector2(sumX / count, sumY / count);
    }
}