using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomTool
{
    private static System.Random enemyRandom;
    private static System.Random bulletRandom;

    public static void InitEnemyRandom(int seed)
    {
        enemyRandom=new System.Random(seed);
    }
    
    public static void InitBulletRandom(int seed)
    {
        bulletRandom=new System.Random(seed);
    }
    
    public static int GetEnemyRandomInt(int min, int max)
    {
        return enemyRandom.Next(min,max);
    }
    
    public static float GetEnemyRandomFloat(float min, float max)
    {
        return (float)(min + enemyRandom.NextDouble() * (max - min));
    }
    
    public static int GetBulletRandomInt(int min, int max)
    {
        return bulletRandom.Next(min,max);
    }
    
    public static float GetBulletRandomFloat(float min, float max)
    {
        return (float)(min + bulletRandom.NextDouble() * (max - min));
    }
}
