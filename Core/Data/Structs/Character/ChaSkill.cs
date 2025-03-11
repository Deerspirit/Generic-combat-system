using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

/// <summary>
/// 技能对象类：表示角色已学习的具体技能实例
/// 包含技能模型引用、技能等级和冷却时间
/// </summary>
public class SkillObj
{
    /// <summary>
    /// 技能模型引用，定义技能的基本属性和效果
    /// </summary>
    public SkillModel model;

    /// <summary>
    /// 技能等级，影响技能效果强度
    /// </summary>
    public int level;

    /// <summary>
    /// 技能当前冷却时间，大于0时无法释放
    /// </summary>
    public float cooldown;

    /// <summary>
    /// 创建技能对象实例
    /// </summary>
    /// <param name="model">技能模型</param>
    /// <param name="level">技能初始等级，默认为1</param>
    public SkillObj(SkillModel model, int level = 1)
    {
        this.model = model;
        this.level = level;
        this.cooldown = 0;
    }
}

/// <summary>
/// 技能模型结构体：定义技能的基本属性和效果
/// 包括技能ID、释放条件、消耗资源、效果和Buff
/// </summary>
public struct SkillModel
{
    /// <summary>
    /// 技能唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// 技能释放条件（如需要的生命值百分比、能量等）
    /// </summary>
    public ChaResource condition;

    /// <summary>
    /// 技能消耗的资源（如魔法值、能量等）
    /// </summary>
    public ChaResource cost;

    /// <summary>
    /// 技能效果的时间线模型，定义技能的执行流程
    /// </summary>
    public TimelineModel effect;

    /// <summary>
    /// 技能施加的Buff信息数组
    /// </summary>
    public AddBuffInfo[] buff;

    /// <summary>
    /// 创建技能模型实例
    /// </summary>
    /// <param name="id">技能唯一标识符</param>
    /// <param name="cost">技能消耗的资源</param>
    /// <param name="condition">技能释放条件</param>
    /// <param name="effectTimeline">技能效果时间线ID</param>
    /// <param name="buff">技能施加的Buff信息数组</param>
    public SkillModel(string id, ChaResource cost, ChaResource condition, string effectTimeline, AddBuffInfo[] buff)
    {
        this.id = id;
        this.cost = cost;
        this.condition = condition;
        this.effect = DesingerTables.Timeline.data[effectTimeline];
        this.buff = buff;
    }
}
