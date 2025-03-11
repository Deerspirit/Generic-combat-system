using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 时间轴管理器：处理技能和效果的时间序列
/// 作为战斗系统的核心组件之一，管理技能释放过程中的各种效果触发
/// </summary>
public class TimelineManager : MonoBehaviour
{
    #region 字段
    /// <summary>
    /// 当前活跃的时间轴列表
    /// </summary>
    private List<TimelineObj> timelines = new List<TimelineObj>();
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 每帧固定更新，处理所有时间轴的进度和事件触发
    /// </summary>
    private void FixedUpdate()
    {
        if (timelines.Count <= 0) 
            return;

        ProcessAllTimelines();
    }
    #endregion

    #region 时间轴处理
    /// <summary>
    /// 处理所有活跃的时间轴
    /// </summary>
    private void ProcessAllTimelines()
    {
        int index = 0;
        while (index < timelines.Count)
        {
            TimelineObj timeline = timelines[index];
            
            // 记录更新前的时间，用于检测事件触发
            float previousTimeElapsed = timeline.timeElapsed;
            
            // 更新时间轴进度
            timeline.timeElapsed += Time.fixedDeltaTime * timeline.timeScale;

            // 处理蓄力返回逻辑
            if (ProcessChargeGoBack(timeline, previousTimeElapsed))
            {
                continue;
            }

            // 触发时间节点事件
            ProcessTimelineNodes(timeline, previousTimeElapsed);

            // 检查时间轴是否结束
            if (timeline.model.duration <= timeline.timeElapsed)
            {
                timelines.RemoveAt(index);
            }
            else
            {
                index++;
            }
        }
    }

    /// <summary>
    /// 处理蓄力返回逻辑
    /// </summary>
    /// <param name="timeline">时间轴对象</param>
    /// <param name="previousTimeElapsed">上一帧的时间进度</param>
    /// <returns>是否触发了蓄力返回</returns>
    private bool ProcessChargeGoBack(TimelineObj timeline, float previousTimeElapsed)
    {
        // 检查是否到达蓄力返回时间点
        if (timeline.model.chargeGoBack.atDuration < timeline.timeElapsed &&
            timeline.model.chargeGoBack.atDuration >= previousTimeElapsed)
        {
            // 检查施放者是否处于蓄力状态
            if (timeline.caster)
            {
                ChaState casterState = timeline.caster.GetComponent<ChaState>();
                if (casterState && casterState.charging)
                {
                    // 返回到指定时间点
                    timeline.timeElapsed = timeline.model.chargeGoBack.gotoDuration;
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 处理时间轴上的节点事件
    /// </summary>
    /// <param name="timeline">时间轴对象</param>
    /// <param name="previousTimeElapsed">上一帧的时间进度</param>
    private void ProcessTimelineNodes(TimelineObj timeline, float previousTimeElapsed)
    {
        foreach (var node in timeline.model.nodes)
        {
            // 检查是否到达节点时间点
            if (node.timeElapsed < timeline.timeElapsed &&
                node.timeElapsed >= previousTimeElapsed)
            {
                // 触发节点事件
                node.doEvent(timeline, node.eveParams);
            }
        }
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 添加新的时间轴
    /// </summary>
    /// <param name="timelineModel">时间轴模型</param>
    /// <param name="caster">施放者</param>
    /// <param name="source">来源对象（如技能）</param>
    public void AddTimeline(TimelineModel timelineModel, GameObject caster, object source)
    {
        // 检查施放者是否已有时间轴
        if (caster != null && CasterHasTimeline(caster)) 
            return;
            
        // 创建并添加新时间轴
        timelines.Add(new TimelineObj(timelineModel, caster, source));
    }

    /// <summary>
    /// 添加已创建的时间轴对象
    /// </summary>
    /// <param name="timeline">时间轴对象</param>
    public void AddTimeline(TimelineObj timeline)
    {
        // 检查施放者是否已有时间轴
        if (timeline.caster != null && CasterHasTimeline(timeline.caster)) 
            return;
            
        // 添加时间轴
        timelines.Add(timeline);
    }

    /// <summary>
    /// 检查施放者是否已有活跃的时间轴
    /// </summary>
    /// <param name="caster">施放者</param>
    /// <returns>是否已有时间轴</returns>
    public bool CasterHasTimeline(GameObject caster)
    {
        foreach (var timeline in timelines)
        {
            if (timeline.caster == caster) 
                return true;
        }
        
        return false;
    }
    #endregion
}