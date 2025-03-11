using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞结果结构体：存储碰撞检测的结果信息
/// 包含是否发生碰撞和碰撞后的推力方向
/// </summary>
public struct CollisionResult
{
    /// <summary>
    /// 是否发生碰撞
    /// </summary>
    public bool hit;

    /// <summary>
    /// 碰撞后应该推向的方向向量
    /// 当hit为true时有效，表示避免碰撞应该移动的方向
    /// </summary>
    public Vector2 pushTo;

    /// <summary>
    /// 创建碰撞结果对象
    /// </summary>
    /// <param name="hit">是否发生碰撞</param>
    /// <param name="pushTo">碰撞推力方向</param>
    public CollisionResult(bool hit, Vector2 pushTo)
    {
        this.hit = hit;
        this.pushTo = pushTo;
    }
}