using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物生成管理器：控制游戏中敌方单位的生成
/// 负责在地图上按一定频率和规则生成敌方单位
/// </summary>
public class MobSpawnManager : MonoBehaviour
{
    #region 公共属性
    /// <summary>
    /// 同时存在的怪物数量上限
    /// </summary>
    [Tooltip("怪物数量的最大值")]
    public int maxMob;

    /// <summary>
    /// 怪物生成周期（秒）
    /// </summary>
    [Tooltip("怪物生成的时间间隔")]
    public float spawnPeriod = 10.0f;
    #endregion

    #region 私有属性
    /// <summary>
    /// 上次生成后经过的时间
    /// </summary>
    private float timeSinceLastSpawn = 0;
    
    /// <summary>
    /// 是否为初次生成
    /// </summary>
    private bool isFirstSpawn = true;
    
    /// <summary>
    /// 已生成的怪物总数（用于难度调整）
    /// </summary>
    private int totalSpawned = 0;
    
    /// <summary>
    /// 怪物阵营标识
    /// </summary>
    private static readonly int ENEMY_SIDE = 2;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 固定更新，处理怪物生成逻辑
    /// </summary>
    private void FixedUpdate()
    {
        // 初次生成怪物
        if (isFirstSpawn && maxMob > 0)
        {
            SpawnMobs();
            isFirstSpawn = false;
        }
        
        // 累计时间并检查是否应该生成
        timeSinceLastSpawn += Time.fixedDeltaTime;
        if (timeSinceLastSpawn >= spawnPeriod)
        {
            timeSinceLastSpawn = 0;
            SpawnMobs();
        }
    }
    #endregion

    #region 怪物生成
    /// <summary>
    /// 生成怪物，数量为（最大数量-当前敌方单位数量）
    /// </summary>
    private void SpawnMobs()
    {
        // 计算当前场景中已有的敌方单位数量
        int currentEnemyCount = CountCurrentEnemies();
        
        // 计算需要生成的数量
        int mobsToSpawn = maxMob - currentEnemyCount;
        
        // 生成怪物
        for (int i = 0; i < mobsToSpawn; i++)
        {
            SpawnSingleMob();
        }
    }

    /// <summary>
    /// 计算当前场景中的敌方单位数量
    /// </summary>
    /// <returns>当前敌方单位数量</returns>
    private int CountCurrentEnemies()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        int count = 0;
        
        foreach (var character in characters)
        {
            ChaState characterState = character.GetComponent<ChaState>();
            if (characterState != null && !characterState.dead && characterState.side == ENEMY_SIDE)
            {
                count++;
            }
        }
        
        return count;
    }

    /// <summary>
    /// 生成单个怪物
    /// </summary>
    private void SpawnSingleMob()
    {
        // 获取随机生成位置
        Vector3 spawnPosition = SceneVariants.map.GetRandomPosForCharacter(
            new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())
        );
        
        // 创建怪物（随机属性和朝向）
        ChaProperty enemyProperty = CreateEnemyProperty();
        float randomRotation = Random.Range(0.00f, 359.99f);
        
        GameObject enemy = SceneVariants.CreateCharacter(
            "MaleGunner", 
            ENEMY_SIDE,
            spawnPosition,
            enemyProperty, 
            randomRotation
        );
        
        // 添加AI控制器
        enemy.AddComponent<SimpleAI>();
        
        // 更新生成计数
        totalSpawned++;
    }

    /// <summary>
    /// 创建敌方单位的属性（随机生成，并随着生成数量增加而变强）
    /// </summary>
    /// <returns>敌方单位属性</returns>
    private ChaProperty CreateEnemyProperty()
    {
        // 基础属性
        float bodyRadius = Random.Range(0.25f, 0.4f);     // 碰撞体半径
        float hitRadius = 0.4f;                          // 受击半径
        
        // 随生成数量递增的属性
        int health = 50 + totalSpawned * 2;              // 生命值
        int attackPower = Random.Range(15, 30) + totalSpawned; // 攻击力
        
        // 随机属性
        int moveSpeed = Random.Range(50, 70);            // 移动速度
        int actionSpeed = 100;                           // 行动速度
        
        return new ChaProperty(
            moveSpeed,    // 移动速度
            health,       // 生命值 
            0,            // 弹药量
            attackPower,  // 攻击力
            actionSpeed,  // 行动速度
            bodyRadius,   // 碰撞体半径
            hitRadius     // 受击半径
        );
    }
    #endregion
}
