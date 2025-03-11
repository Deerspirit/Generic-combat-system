using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Y轴旋转特效：使游戏对象围绕Y轴（上方向）持续旋转
/// 常用于物品展示、光环效果、UI元素等需要持续旋转的视觉效果
/// </summary>
public class RotateY : SightEffect
{
    /// <summary>
    /// 每秒旋转的角度
    /// 正值为顺时针旋转，负值为逆时针旋转
    /// </summary>
    [Tooltip("每秒转多少度（角度）")]
    public float rotatePerSec = 360;

    /// <summary>
    /// 当前累计旋转角度
    /// </summary>
    private float currentDegree = 0;

    /// <summary>
    /// 每帧更新旋转角度
    /// 考虑父级旋转，确保旋转效果不受父级影响
    /// </summary>
    private void Update()
    {
        // 累加旋转角度并保持在0-360范围内
        currentDegree = (currentDegree + rotatePerSec * Time.deltaTime) % 360;
        
        // 计算需要旋转的角度，考虑当前物体已有的旋转
        float shouldRotate = currentDegree - transform.eulerAngles.y;
        
        // 补偿所有父级的旋转，确保旋转效果不受父级影响
        Transform t = this.transform;
        while (t.parent != null)
        {
            shouldRotate -= t.parent.eulerAngles.y;
            t = t.parent;
        }
        
        // 执行旋转
        this.transform.RotateAround(this.transform.position, Vector3.up, shouldRotate);
    }
}