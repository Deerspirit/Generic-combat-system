using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Y轴弹跳球效果：模拟物体在Y轴（高度）方向的弹跳效果
/// 可以设置弹跳高度和落地时间点，实现自然的弹跳动画
/// 每次弹跳高度会逐渐降低，模拟能量损失
/// </summary>
public class BouncingBallY : SightEffect
{
    /// <summary>
    /// 动画已经运行的总时间
    /// </summary>
    private float timeElapsed = 0;

    /// <summary>
    /// 第一次弹跳的最高点高度（米）
    /// </summary>
    [Tooltip("弹跳的最高点，单位：米")]
    public float highestPoint = 4.0f;

    /// <summary>
    /// 落地时间点数组，记录每次落地的时间（秒）
    /// 例如：[1.0, 2.5, 3.5]表示在1秒、2.5秒和3.5秒时落地
    /// </summary>
    [Tooltip("落点反弹的时间点，因为落地和时间有关，单位：秒")]
    public float[] hitGroundAt = new float[0];

    /// <summary>
    /// 当前所处的弹跳阶段索引
    /// </summary>
    private int partIndex = 0;

    /// <summary>
    /// 每帧更新弹跳动画
    /// </summary>
    private void Update()
    {
        // 如果没有设置落地时间点，则不执行弹跳
        if (this.hitGroundAt.Length <= 0) return;

        float timePassed = Time.deltaTime;

        // 更新当前所处的弹跳阶段
        while (partIndex < hitGroundAt.Length && timeElapsed >= hitGroundAt[partIndex])
        {
            partIndex += 1;
        }

        // 如果已经完成所有弹跳阶段，将物体放回地面并结束动画
        if (partIndex >= hitGroundAt.Length)
        {
            this.transform.position = new Vector3(
                this.transform.position.x,
                0,
                this.transform.position.z
            );
            this.hitGroundAt = new float[0];
            return;
        }

        // 计算当前弹跳阶段的时间参数
        // partTime: 当前弹跳阶段的总时长
        // cpTime: 在当前弹跳阶段已经过去的时间
        // tPerc: 当前弹跳阶段的完成百分比(0-1)
        float partTime = Mathf.Max(0.001f, hitGroundAt[partIndex] - (partIndex <= 0 ? 0 : hitGroundAt[partIndex - 1]));
        float cpTime = timeElapsed - (partIndex <= 0 ? 0 : hitGroundAt[partIndex - 1]);
        float tPerc = Mathf.Min(cpTime / partTime, 1.000f);
        
        // 判断是在上升阶段还是下降阶段
        bool isRising = tPerc < 0.5f;
        
        // 计算当前弹跳的最高点（每次弹跳高度减半，模拟能量损失）
        float currentMaxHeight = highestPoint / Mathf.Pow(2, partIndex);
        
        // 使用正弦函数计算当前高度
        float currentHeight = Mathf.Sin(tPerc * Mathf.PI) * currentMaxHeight;
        
        // 更新物体位置，确保上升/下降过程平滑
        this.transform.position = new Vector3(
            this.transform.position.x,
            isRising ? Mathf.Max(this.transform.position.y, currentHeight) : Mathf.Min(this.transform.position.y, currentHeight),
            this.transform.position.z
        );

        // 累加已经过去的时间
        this.timeElapsed += timePassed;
    }

    /// <summary>
    /// 重置弹跳动画参数并开始新的弹跳
    /// </summary>
    /// <param name="highest">初始弹跳最高点（米）</param>
    /// <param name="hitGroundTime">落地时间点数组（秒）</param>
    public void ResetTo(float highest, float[] hitGroundTime)
    {
        this.hitGroundAt = hitGroundTime;
        this.highestPoint = highest;
        this.partIndex = 0;
        this.timeElapsed = 0;
    }
}