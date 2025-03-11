using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 球体滚动效果：使球体根据父物体的移动自动产生滚动旋转效果
/// 模拟球体在地面上滚动的物理效果，适用于各种球形物体
/// </summary>
public class BallRolling : SightEffect
{
    /// <summary>
    /// 父物体上一帧的位置，用于计算移动距离
    /// </summary>
    private Vector3 previousParentPosition = new Vector3();

    /// <summary>
    /// 球体的渲染器组件，用于获取球体半径
    /// </summary>
    private Renderer renderer;
    
    /// <summary>
    /// 初始化组件，记录初始位置并获取渲染器
    /// </summary>
    private void Start()
    {
        // 记录父物体初始位置
        previousParentPosition = this.transform.parent.position;
        
        // 获取渲染器组件（直接或从子物体获取）
        renderer = this.gameObject.GetComponent<Renderer>();
        if (!renderer) renderer = this.gameObject.GetComponentInChildren<Renderer>();
    }

    /// <summary>
    /// 每帧更新球体的旋转，根据父物体的移动计算
    /// </summary>
    private void Update()
    {
        // 如果没有渲染器，无法计算半径，直接返回
        if (!renderer) return;
        
        // 计算父物体的移动距离
        Vector3 movementDelta = this.transform.parent.position - previousParentPosition;
        
        // 获取球体半径（使用渲染边界的一半）
        float radius = renderer.bounds.size.x / 2;
        
        // 计算X轴和Z轴的旋转角度
        // 公式：角度 = 移动距离 * 180° / (π * 半径) - 父物体当前旋转角度
        float rotationX = movementDelta.x * 180.00f / (Mathf.PI * radius) - this.transform.parent.eulerAngles.x;
        float rotationZ = movementDelta.z * 180.00f / (Mathf.PI * radius) - this.transform.parent.eulerAngles.z;
        
        // 应用旋转（右轴旋转对应Z轴移动，后轴旋转对应X轴移动）
        transform.RotateAround(transform.position, Vector3.right, rotationX);
        transform.RotateAround(transform.position, Vector3.back, rotationZ);
        
        // 更新上一帧位置
        previousParentPosition = this.transform.parent.position;
    }
}