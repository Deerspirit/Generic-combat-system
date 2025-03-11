using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单位弹出文本：控制在角色头顶弹出并上升的文本效果
/// 用于显示伤害、治疗、状态等信息，并随时间淡出
/// </summary>
public class UnitPopText : MonoBehaviour
{
    /// <summary>
    /// 文本效果的总持续时间（秒）
    /// </summary>
    private static float totalDuration = 1.50f;
    
    /// <summary>
    /// 当前剩余持续时间
    /// </summary>
    private float duration = totalDuration;

    /// <summary>
    /// 文本上升的最大高度（屏幕像素）
    /// </summary>
    [Tooltip("文字最终飘多高")]
    public float popHeight = 10.000f;

    /// <summary>
    /// 文本跟随的目标角色
    /// </summary>
    [Tooltip("在谁头上跳")]
    public GameObject target;

    /// <summary>
    /// 每帧更新文本位置和透明度
    /// </summary>
    private void Update()
    {
        // 如果目标不存在，不执行更新
        if (!target) return;

        float timePassed = Time.deltaTime;

        // 获取目标角色在屏幕上的位置
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, target.transform.position);
        
        // 计算当前动画进度（0-1）
        float progress = (totalDuration - duration) / totalDuration;
        
        // 使用缓动函数计算当前高度
        float currentHeight = ease(progress) * popHeight;
        
        // 更新文本位置
        this.transform.position = screenPosition + Vector2.up * currentHeight;

        // 更新剩余时间，并在时间结束时销毁对象
        duration -= timePassed;
        if (duration <= 0) Destroy(this.gameObject);
    }

    /// <summary>
    /// 缓动函数：使文本上升速度随时间变化，开始快然后减慢
    /// </summary>
    /// <param name="t">动画进度（0-1）</param>
    /// <returns>缓动后的值（0-1）</returns>
    private float ease(float t)
    {
        // 确保t在0-1范围内
        t = Mathf.Clamp(t, 0.000f, 1.000f);
        
        // 使用平方根函数实现缓动效果
        return Mathf.Sqrt(t);
    }
}