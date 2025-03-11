using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位旋转系统：控制游戏中单位的旋转行为
/// 提供平滑旋转和立即旋转功能，支持多种旋转方式
/// </summary>
public class UnitRotate : MonoBehaviour
{
    #region 公共属性
    /// <summary>
    /// 旋转速度（度/秒）
    /// </summary>
    [Tooltip("单位的旋转速度，单位：度/秒")]
    public float rotateSpeed = 180f;

    /// <summary>
    /// 是否使用平滑旋转（false则立即旋转）
    /// </summary>
    [Tooltip("是否使用平滑旋转，如果为false则立即旋转到目标角度")]
    public bool useSmoothRotation = true;
    #endregion

    #region 私有属性
    /// <summary>
    /// 当前是否可以旋转
    /// </summary>
    private bool canRotate = true;

    /// <summary>
    /// 目标旋转角度
    /// </summary>
    private float targetDegree = 0.00f;

    /// <summary>
    /// 旋转完成的最小阈值（度）
    /// </summary>
    private const float RotationThreshold = 0.01f;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 固定更新，处理单位旋转逻辑
    /// </summary>
    void FixedUpdate()
    {
        // 检查是否可以旋转，以及是否已完成旋转
        if (!canRotate || IsRotationComplete()) 
            return;

        if (useSmoothRotation)
        {
            // 平滑旋转
            PerformSmoothRotation();
        }
        else
        {
            // 立即旋转
            PerformInstantRotation();
        }
    }
    #endregion

    #region 旋转计算
    /// <summary>
    /// 执行平滑旋转
    /// </summary>
    private void PerformSmoothRotation()
    {
        // 获取当前Y轴旋转角度，规范化到-180到180度范围
        float currentDegree = transform.rotation.eulerAngles.y;
        if (currentDegree > 180.00f) 
            currentDegree -= 360.00f;
        
        // 计算最短旋转路径
        float directDistance = targetDegree - currentDegree;
        float alternativeDistance = targetDegree > currentDegree 
            ? (targetDegree - 360.00f - currentDegree) 
            : (targetDegree + 360.00f - currentDegree);
        
        // 确定旋转方向（顺时针或逆时针）
        bool rotateNegative = Mathf.Abs(directDistance) < Mathf.Abs(alternativeDistance) 
            ? (directDistance < 0) 
            : (alternativeDistance < 0);
        
        // 计算本帧的旋转量，不超过目标距离和最大旋转速度
        float rotationAmount = Mathf.Min(
            rotateSpeed * Time.fixedDeltaTime, 
            Mathf.Abs(directDistance), 
            Mathf.Abs(alternativeDistance)
        );
        
        // 应用旋转方向
        if (rotateNegative) 
            rotationAmount *= -1;
        
        // 执行旋转
        transform.Rotate(new Vector3(0, rotationAmount, 0));
    }

    /// <summary>
    /// 执行立即旋转
    /// </summary>
    private void PerformInstantRotation()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = targetDegree;
        transform.eulerAngles = eulerAngles;
    }

    /// <summary>
    /// 检查旋转是否已完成
    /// </summary>
    /// <returns>是否已完成旋转</returns>
    private bool IsRotationComplete()
    {
        float rotationDelta = Mathf.Abs(NormalizeAngle(transform.rotation.eulerAngles.y) - NormalizeAngle(targetDegree));
        float minRotationThreshold = Mathf.Min(RotationThreshold, rotateSpeed * Time.fixedDeltaTime);
        return rotationDelta < minRotationThreshold;
    }

    /// <summary>
    /// 规范化角度到-180到180度范围
    /// </summary>
    /// <param name="angle">输入角度</param>
    /// <returns>规范化的角度</returns>
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f)
            angle -= 360f;
        else if (angle < -180f)
            angle += 360f;
        return angle;
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 旋转到指定角度
    /// </summary>
    /// <param name="degree">目标角度（度）</param>
    public void RotateTo(float degree)
    {
        targetDegree = degree;
        
        if (!useSmoothRotation && canRotate)
        {
            PerformInstantRotation();
        }
    }

    /// <summary>
    /// 旋转到指定向量方向
    /// </summary>
    /// <param name="x">向量X分量</param>
    /// <param name="z">向量Z分量</param>
    public void RotateTo(float x, float z)
    {
        float degree = Mathf.Atan2(x, z) * Mathf.Rad2Deg;
        RotateTo(degree);
    }

    /// <summary>
    /// 旋转指定的角度（相对当前角度）
    /// </summary>
    /// <param name="degree">旋转角度（度）</param>
    public void RotateBy(float degree)
    {
        RotateTo(transform.rotation.eulerAngles.y + degree);
    }

    /// <summary>
    /// 禁用旋转能力
    /// </summary>
    public void DisableRotate()
    {
        canRotate = false;
        targetDegree = transform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 启用旋转能力
    /// </summary>
    public void EnableRotate()
    {
        canRotate = true;
    }

    /// <summary>
    /// 设置旋转速度
    /// </summary>
    /// <param name="speed">旋转速度（度/秒）</param>
    public void SetRotateSpeed(float speed)
    {
        rotateSpeed = Mathf.Max(0, speed);
    }

    /// <summary>
    /// 获取当前的旋转角度
    /// </summary>
    /// <returns>当前旋转角度（度）</returns>
    public float GetCurrentRotation()
    {
        return transform.rotation.eulerAngles.y;
    }
    #endregion
}
