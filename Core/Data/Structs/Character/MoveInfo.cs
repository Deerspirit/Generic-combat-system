using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动预设指令：表示一个预定义的移动指令，如击退、冲刺等
/// 包含速度向量和持续时间，用于实现非玩家控制的移动效果
/// </summary>
public class MovePreorder
{
    /// <summary>
    /// 移动速度向量，表示移动的方向和速度
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// 初始指令的总持续时间，用于计算速度衰减
    /// </summary>
    private float initialDuration;

    /// <summary>
    /// 当前剩余持续时间
    /// </summary>
    public float duration;

    /// <summary>
    /// 创建一个移动预设指令
    /// </summary>
    /// <param name="velocity">移动速度向量</param>
    /// <param name="duration">持续时间（秒）</param>
    public MovePreorder(Vector3 velocity, float duration)
    {
        this.velocity = velocity;
        this.duration = duration;
        this.initialDuration = duration;
    }

    /// <summary>
    /// 计算指定时间内应该应用的速度
    /// 同时减少剩余时间，当时间结束时返回零向量
    /// </summary>
    /// <param name="deltaTime">时间增量</param>
    /// <returns>应用的速度向量</returns>
    public Vector3 VeloInTime(float deltaTime)
    {
        // 如果时间超过了持续时间，标记为已完成
        if (deltaTime >= duration)
        {
            this.duration = 0;
        }
        else
        {
            // 减少剩余持续时间
            this.duration -= deltaTime;
        }
        
        // 返回适当的速度向量
        // 如果初始持续时间为0，直接返回速度向量
        // 否则根据初始持续时间计算平均速度
        return initialDuration <= 0 ? velocity : (velocity / initialDuration);
    }
}

/// <summary>
/// 移动类型枚举：定义角色的移动模式
/// </summary>
public enum MoveType
{
    /// <summary>
    /// 地面移动：受到地形和重力影响
    /// </summary>
    ground = 0,
    
    /// <summary>
    /// 飞行移动：不受地形和重力影响
    /// </summary>
    fly = 1
}
