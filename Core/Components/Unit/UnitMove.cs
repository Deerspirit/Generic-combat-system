using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位移动系统：控制游戏中单位的移动行为和碰撞
/// 支持不同移动类型和碰撞检测方式，与地图系统紧密结合
/// </summary>
public class UnitMove : MonoBehaviour
{
    #region 公共属性
    /// <summary>
    /// 单位的移动类型（地面、飞行等）
    /// </summary>
    [Tooltip("单位的移动类型，根据游戏设计不同，这个值也可以不同")]
    public MoveType moveType = MoveType.ground;

    /// <summary>
    /// 单位的碰撞体半径
    /// </summary>
    [Tooltip("单位的移动体型碰撞圆形的半径，单位：米")]
    public float bodyRadius = 0.25f;

    /// <summary>
    /// 是否使用平滑移动
    /// </summary>
    [Tooltip("当单位移动被地图阻挡的时候，是选择一个更好的落脚点（true）还是直接停止移动（false），如果直接停止移动，那么停下的时候访问hitObstacle的时候就是true，否则hitObstacle永远是false")]
    public bool smoothMove = true;

    /// <summary>
    /// 是否忽略地图边界
    /// </summary>
    [Tooltip("是否会忽略关卡外围，即飞行（只有飞行允许）到地图外的地方全部视作可过")]
    public bool ignoreBorder = true;

    /// <summary>
    /// 是否碰撞到障碍物
    /// </summary>
    public bool hitObstacle
    {
        get { return _hitObstacle; }
        private set { _hitObstacle = value; }
    }
    #endregion

    #region 私有属性
    /// <summary>
    /// 当前是否可以移动
    /// </summary>
    private bool canMove = true;

    /// <summary>
    /// 碰撞到障碍物的内部标记
    /// </summary>
    private bool _hitObstacle = false;

    /// <summary>
    /// 当前移动速度向量
    /// </summary>
    private Vector3 velocity = Vector3.zero;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 固定更新，处理单位移动逻辑
    /// </summary>
    void FixedUpdate()
    {
        // 检查是否可以移动
        if (!canMove || velocity == Vector3.zero) 
            return;

        // 计算下一帧的目标位置
        Vector3 targetPosition = CalculateTargetPosition();

        // 使用地图系统修正目标位置（处理碰撞）
        MapTargetPosInfo positionInfo = GetCorrectedPosition(targetPosition);

        // 处理非平滑移动时的碰撞停止
        if (!smoothMove && positionInfo.obstacle)
        {
            hitObstacle = true;
            canMove = false;
        }

        // 应用移动结果
        transform.position = positionInfo.suggestPos;

        // 清除本帧的移动力
        ResetVelocity();
    }
    #endregion

    #region 移动计算
    /// <summary>
    /// 计算目标位置
    /// </summary>
    /// <returns>下一帧的目标位置</returns>
    private Vector3 CalculateTargetPosition()
    {
        return new Vector3(
            velocity.x * Time.fixedDeltaTime + transform.position.x,
            velocity.y * Time.fixedDeltaTime + transform.position.y,
            velocity.z * Time.fixedDeltaTime + transform.position.z
        );
    }

    /// <summary>
    /// 获取经过碰撞修正的位置
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <returns>经过修正的位置信息</returns>
    private MapTargetPosInfo GetCorrectedPosition(Vector3 targetPos)
    {
        bool ignoreMapBorder = ignoreBorder && moveType == MoveType.fly;
        
        return SceneVariants.map.FixTargetPosition(
            transform.position, 
            bodyRadius, 
            targetPos, 
            moveType, 
            ignoreMapBorder
        );
    }

    /// <summary>
    /// 重置移动速度
    /// </summary>
    private void ResetVelocity()
    {
        velocity = Vector3.zero;
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 获取当前移动方向
    /// </summary>
    /// <returns>移动方向向量</returns>
    public Vector3 GetMoveDirection()
    {
        return velocity.normalized;
    }

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    /// <returns>移动速度向量</returns>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    /// <summary>
    /// 设置移动力
    /// </summary>
    /// <param name="moveForce">移动力向量</param>
    public void MoveBy(Vector3 moveForce)
    {
        if (!canMove) 
            return;

        velocity = moveForce;
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public void StopMoving()
    {
        ResetVelocity();
    }

    /// <summary>
    /// 禁用移动能力
    /// </summary>
    public void DisableMove()
    {
        StopMoving();
        canMove = false;
    }

    /// <summary>
    /// 启用移动能力
    /// </summary>
    public void EnableMove()
    {
        canMove = true;
        hitObstacle = false;
    }
    #endregion
}
