using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色饼图控制器：用于显示角色的生命值饼状图
/// 根据角色当前生命值百分比，动态调整饼图的角度范围
/// </summary>
public class ChaPie : MonoBehaviour
{
    /// <summary>
    /// 角色状态组件引用
    /// </summary>
    private ChaState chaState;
    
    /// <summary>
    /// 饼图控制器组件引用
    /// </summary>
    private PieChartController chart;

    /// <summary>
    /// 初始化组件引用并设置饼图初始半径
    /// </summary>
    private void Start()
    {
        // 获取必要组件
        chaState = this.gameObject.GetComponent<ChaState>();
        chart = this.gameObject.GetComponent<PieChartController>();

        // 检查组件是否存在
        if (!chaState || !chart)
            return;
            
        // 根据角色碰撞体半径设置饼图大小
        chart.radius = chaState.property.bodyRadius;
    }

    /// <summary>
    /// 每帧更新饼图角度和旋转，以匹配角色状态
    /// </summary>
    private void FixedUpdate()
    {
        // 检查组件是否存在
        if (!chaState || !chart)
            return;

        // 根据生命值百分比更新饼图角度
        chart.angleDegree = 360 * chaState.resource.hp / chaState.property.hp;
        
        // 调整饼图旋转，使其保持在水平面上（不跟随角色旋转）
        chart.transform.localEulerAngles = new Vector3(
            chart.transform.localRotation.eulerAngles.x,
            -this.transform.eulerAngles.y,
            chart.transform.localRotation.eulerAngles.z
        );
    }
}
