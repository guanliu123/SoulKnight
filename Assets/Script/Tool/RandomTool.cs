using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomTool
{
    private static System.Random enemyRandom;
    private static int enemyRandomCnt = 0;
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
         enemyRandomCnt++;
        Debug.Log("GetEnemyRandomInt 敌人随机整数:"+ enemyRandomCnt);
        return enemyRandom.Next(min,max);
    }
    
    public static float GetEnemyRandomFloat(float min, float max)
    {
        enemyRandomCnt++;
        Debug.Log("GetEnemyRandomFloat 敌人随机浮点数:"+ enemyRandomCnt);
        return (float)(min + enemyRandom.NextDouble() * (max - min));
    }
    
    public static int GetBulletRandomInt(int min, int max)
    {
        if (bulletRandom == null)
        {
            RandomTool.InitBulletRandom(-1);
        }
        return bulletRandom.Next(min,max);
    }
    
    public static float GetBulletRandomFloat(float min, float max)
    {
        if (bulletRandom == null)
        {
            RandomTool.InitBulletRandom(-1);
        }
        return (float)(min + bulletRandom.NextDouble() * (max - min));
    }
}
