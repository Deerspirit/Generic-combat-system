using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单AI控制系统：控制NPC的基本行为
/// 实现基础的移动、旋转和攻击行为，可用于敌人和友方NPC
/// </summary>
public class SimpleAI : MonoBehaviour
{
    #region 私有属性
    /// <summary>
    /// 下次开火的倒计时（秒）
    /// </summary>
    private float fireCountdown = 3.0f;
    
    /// <summary>
    /// 下次改变移动方向的倒计时（秒）
    /// </summary>
    private float rotateCountdown = 2.0f;
    
    /// <summary>
    /// 当前移动方向角度
    /// </summary>
    private float movementDirection;
    
    /// <summary>
    /// 角色状态组件引用
    /// </summary>
    private ChaState characterState;
    
    /// <summary>
    /// 攻击间隔的最小值（秒）
    /// </summary>
    [SerializeField] 
    private float minFireInterval = 2.0f;
    
    /// <summary>
    /// 攻击间隔的最大值（秒）
    /// </summary>
    [SerializeField] 
    private float maxFireInterval = 5.0f;
    
    /// <summary>
    /// 移动方向变更的最小间隔（秒）
    /// </summary>
    [SerializeField] 
    private float minRotateInterval = 1.6f;
    
    /// <summary>
    /// 移动方向变更的最大间隔（秒）
    /// </summary>
    [SerializeField] 
    private float maxRotateInterval = 3.2f;
    
    /// <summary>
    /// 移动方向变更的最大角度
    /// </summary>
    [SerializeField] 
    private float maxRotateAngle = 90.0f;
    #endregion

    #region 行为定义
    /// <summary>
    /// 开火行为的时间轴模型
    /// </summary>
    private readonly TimelineModel fireAction = new TimelineModel(
        "", 
        new TimelineNode[] {
            // 禁用技能使用权限
            new TimelineNode(0.00f, "SetCasterControlState", new object[] { true, true, false }),
            
            // 播放开火动画
            new TimelineNode(0.00f, "CasterPlayAnim", new object[] { "Fire", false }),
            
            // 播放枪口特效
            new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[] { "Muzzle", "Effect/MuzzleFlash", "", false }),
            
            // 发射子弹
            new TimelineNode(0.10f, "FireBullet", new object[] {
                new BulletLauncher(
                    DesingerTables.Bullet.data["normal1"], 
                    null,               // 目标函数在这里设置为null，将在时间轴执行时动态获取
                    Vector3.zero,       // 位置将在时间轴执行时动态设置
                    0,                  // 角度将在时间轴执行时动态设置 
                    6.0f,               // 子弹速度
                    10.0f,              // 子弹持续时间
                    0,                  // 子弹无法命中时间
                    null,               // 无自定义参数
                    null,               // 无自定义轨迹函数
                    false               // 不固定发射角度
                ), 
                "Muzzle"  // 从枪口发射
            }),
            
            // 恢复技能使用权限
            new TimelineNode(0.50f, "SetCasterControlState", new object[] { true, true, true })
        }, 
        0.50f,            // 时间轴总持续时间
        TimelineGoTo.Null // 无循环
    );
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 初始化AI
    /// </summary>
    void Start()
    {
        // 获取角色状态组件
        characterState = GetComponent<ChaState>();
        
        // 初始化移动方向为当前朝向
        movementDirection = transform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 固定更新，处理AI行为
    /// </summary>
    private void FixedUpdate()
    {
        // 检查角色是否有效
        if (characterState == null || characterState.dead)
            return;

        float deltaTime = Time.fixedDeltaTime;

        // 执行AI行为
        FaceTowardsPlayer();
        UpdateMovement(deltaTime);
        UpdateAttack(deltaTime);
    }
    #endregion

    #region AI行为
    /// <summary>
    /// 转向玩家
    /// </summary>
    private void FaceTowardsPlayer()
    {
        // 获取玩家位置
        GameObject player = SceneVariants.MainActor();
        if (player == null)
            return;
            
        // 计算朝向玩家的角度
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float targetRotation = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
        
        // 命令角色旋转
        characterState.OrderRotateTo(targetRotation);
    }

    /// <summary>
    /// 更新移动行为
    /// </summary>
    /// <param name="deltaTime">时间增量</param>
    private void UpdateMovement(float deltaTime)
    {
        // 更新移动方向计时器
        rotateCountdown -= deltaTime;
        
        // 定期改变移动方向
        if (rotateCountdown <= 0)
        {
            // 随机调整移动方向
            movementDirection += Random.Range(-maxRotateAngle, maxRotateAngle);
            
            // 重置计时器，随机设置下次改变方向的时间
            rotateCountdown = Random.Range(minRotateInterval, maxRotateInterval);
        }
        
        // 计算移动向量
        float moveRadians = movementDirection * Mathf.Deg2Rad;
        float moveSpeed = characterState.moveSpeed;
        Vector3 moveVector = new Vector3(
            Mathf.Sin(moveRadians) * moveSpeed,
            0,
            Mathf.Cos(moveRadians) * moveSpeed
        );
        
        // 命令角色移动
        characterState.OrderMove(moveVector);
    }

    /// <summary>
    /// 更新攻击行为
    /// </summary>
    /// <param name="deltaTime">时间增量</param>
    private void UpdateAttack(float deltaTime)
    {
        // 更新攻击计时器
        fireCountdown -= deltaTime;
        
        // 定期执行攻击
        if (fireCountdown <= 0)
        {
            // 创建攻击时间轴
            SceneVariants.CreateTimeline(fireAction, gameObject, null);
            
            // 重置计时器，随机设置下次攻击的时间
            fireCountdown = Random.Range(minFireInterval, maxFireInterval);
        }
    }
    #endregion
}