using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff添加信息结构体：包含添加Buff所需的所有参数
/// 用于在角色之间传递Buff信息，包括施加者、目标、层数和持续时间等
/// </summary>
public struct AddBuffInfo
{
    /// <summary>
    /// Buff的施加者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// Buff的目标
    /// </summary>
    public GameObject target;

    /// <summary>
    /// Buff模型，定义Buff的基本属性和效果
    /// </summary>
    public BuffModel buffModel;

    /// <summary>
    /// 添加的Buff层数
    /// </summary>
    public int addStack;

    /// <summary>
    /// 是否强制设置持续时间（true=设置为指定值，false=增加指定值）
    /// </summary>
    public bool durationSetTo;

    /// <summary>
    /// 是否永久持续（不会随时间减少）
    /// </summary>
    public bool permanent;

    /// <summary>
    /// Buff持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// Buff参数字典，用于传递自定义参数
    /// </summary>
    public Dictionary<string, object> buffParam;

    /// <summary>
    /// 创建Buff添加信息
    /// </summary>
    /// <param name="model">Buff模型</param>
    /// <param name="caster">施加者</param>
    /// <param name="target">目标</param>
    /// <param name="stack">层数</param>
    /// <param name="duration">持续时间</param>
    /// <param name="durationSetTo">是否设置持续时间</param>
    /// <param name="permanent">是否永久</param>
    /// <param name="buffParam">自定义参数</param>
    public AddBuffInfo(
        BuffModel model, GameObject caster, GameObject target,
        int stack, float duration, bool durationSetTo = true,
        bool permanent = false,
        Dictionary<string, object> buffParam = null
    )
    {
        this.buffModel = model;
        this.caster = caster;
        this.target = target;
        this.addStack = stack;
        this.duration = duration;
        this.durationSetTo = durationSetTo;
        this.buffParam = buffParam;
        this.permanent = permanent;
    }
}

/// <summary>
/// Buff对象类：表示一个已经应用在角色身上的具体Buff实例
/// 包括Buff模型、持续时间、层数和各种状态信息
/// </summary>
public class BuffObj
{
    /// <summary>
    /// Buff模型，定义Buff的基本属性和效果
    /// </summary>
    public BuffModel model;

    /// <summary>
    /// 剩余持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// 是否永久持续（不会随时间减少）
    /// </summary>
    public bool permanent;

    /// <summary>
    /// 当前层数
    /// </summary>
    public int stack;

    /// <summary>
    /// Buff的施加者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// Buff的携带者（目标）
    /// </summary>
    public GameObject carrier;

    /// <summary>
    /// Buff已经存在的时间（秒）
    /// </summary>
    public float timeElapsed = 0.00f;

    /// <summary>
    /// 已触发的周期效果次数
    /// </summary>
    public int ticked = 0;

    /// <summary>
    /// Buff自定义参数字典
    /// </summary>
    public Dictionary<string, object> buffParam = new Dictionary<string, object>();

    /// <summary>
    /// 创建Buff对象
    /// </summary>
    /// <param name="model">Buff模型</param>
    /// <param name="caster">施加者</param>
    /// <param name="carrier">携带者（目标）</param>
    /// <param name="duration">持续时间</param>
    /// <param name="stack">层数</param>
    /// <param name="permanent">是否永久</param>
    /// <param name="buffParam">自定义参数</param>
    public BuffObj(
        BuffModel model, GameObject caster, GameObject carrier, float duration, int stack, bool permanent = false,
        Dictionary<string, object> buffParam = null
    )
    {
        this.model = model;
        this.caster = caster;
        this.carrier = carrier;
        this.duration = duration;
        this.stack = stack;
        this.permanent = permanent;
        if (buffParam != null)
        {
            foreach (KeyValuePair<string, object> kv in buffParam)
            {
                this.buffParam.Add(kv.Key, kv.Value);
            }
        }
    }
}

/// <summary>
/// Buff模型结构体：定义Buff的基本属性和行为
/// 包括ID、名称、优先级、最大层数、触发条件和回调函数等
/// </summary>
public struct BuffModel
{
    /// <summary>
    /// Buff唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// Buff显示名称
    /// </summary>
    public string name;

    /// <summary>
    /// Buff优先级，影响Buff的应用顺序
    /// </summary>
    public int priority;

    /// <summary>
    /// Buff最大叠加层数
    /// </summary>
    public int maxStack;

    /// <summary>
    /// Buff标签数组，用于分类和筛选
    /// </summary>
    public string[] tags;

    /// <summary>
    /// Buff周期效果触发间隔（秒）
    /// </summary>
    public float tickTime;

    /// <summary>
    /// Buff属性修改器 [0]=加值修改，[1]=乘值修改（百分比）
    /// </summary>
    public ChaProperty[] propMod;

    /// <summary>
    /// Buff状态修改器，影响角色的控制状态
    /// </summary>
    public ChaControlState stateMod;

    /// <summary>
    /// Buff应用时的回调函数
    /// </summary>
    public BuffOnOccur onOccur;
    
    /// <summary>
    /// Buff应用时的回调函数参数
    /// </summary>
    public object[] onOccurParams;

    /// <summary>
    /// Buff周期触发的回调函数
    /// </summary>
    public BuffOnTick onTick;
    
    /// <summary>
    /// Buff周期触发的回调函数参数
    /// </summary>
    public object[] onTickParams;

    /// <summary>
    /// Buff移除时的回调函数
    /// </summary>
    public BuffOnRemoved onRemoved;
    
    /// <summary>
    /// Buff移除时的回调函数参数
    /// </summary>
    public object[] onRemovedParams;

    /// <summary>
    /// 角色施放技能时的回调函数
    /// </summary>
    public BuffOnCast onCast;
    
    /// <summary>
    /// 角色施放技能时的回调函数参数
    /// </summary>
    public object[] onCastParams;

    /// <summary>
    /// 角色造成伤害时的回调函数
    /// </summary>
    public BuffOnHit onHit;
    
    /// <summary>
    /// 角色造成伤害时的回调函数参数
    /// </summary>
    public object[] onHitParams;

    /// <summary>
    /// 角色受到伤害时的回调函数
    /// </summary>
    public BuffOnBeHurt onBeHurt;
    
    /// <summary>
    /// 角色受到伤害时的回调函数参数
    /// </summary>
    public object[] onBeHurtParams;

    /// <summary>
    /// 角色击杀目标时的回调函数
    /// </summary>
    public BuffOnKill onKill;
    
    /// <summary>
    /// 角色击杀目标时的回调函数参数
    /// </summary>
    public object[] onKillParams;

    /// <summary>
    /// 角色被击杀时的回调函数
    /// </summary>
    public BuffOnBeKilled onBeKilled;
    
    /// <summary>
    /// 角色被击杀时的回调函数参数
    /// </summary>
    public object[] onBeKilledParams;

    /// <summary>
    /// 创建Buff模型
    /// </summary>
    /// <param name="id">Buff唯一标识符</param>
    /// <param name="name">Buff显示名称</param>
    /// <param name="tags">Buff标签数组</param>
    /// <param name="priority">Buff优先级</param>
    /// <param name="maxStack">Buff最大叠加层数</param>
    /// <param name="tickTime">Buff周期效果触发间隔（秒）</param>
    /// <param name="onOccur">Buff应用时的回调函数名称</param>
    /// <param name="occurParam">Buff应用时的回调函数参数</param>
    /// <param name="onRemoved">Buff移除时的回调函数名称</param>
    /// <param name="removedParam">Buff移除时的回调函数参数</param>
    /// <param name="onTick">Buff周期触发的回调函数名称</param>
    /// <param name="tickParam">Buff周期触发的回调函数参数</param>
    /// <param name="onCast">角色施放技能时的回调函数名称</param>
    /// <param name="castParam">角色施放技能时的回调函数参数</param>
    /// <param name="onHit">角色造成伤害时的回调函数名称</param>
    /// <param name="hitParam">角色造成伤害时的回调函数参数</param>
    /// <param name="beHurt">角色受到伤害时的回调函数名称</param>
    /// <param name="hurtParam">角色受到伤害时的回调函数参数</param>
    /// <param name="onKill">角色击杀目标时的回调函数名称</param>
    /// <param name="killParam">角色击杀目标时的回调函数参数</param>
    /// <param name="beKilled">角色被击杀时的回调函数名称</param>
    /// <param name="beKilledParam">角色被击杀时的回调函数参数</param>
    /// <param name="stateMod">Buff状态修改器</param>
    /// <param name="propMod">Buff属性修改器数组</param>
    public BuffModel(
        string id, string name, string[] tags, int priority, int maxStack, float tickTime,
        string onOccur, object[] occurParam,
        string onRemoved, object[] removedParam,
        string onTick, object[] tickParam,
        string onCast, object[] castParam,
        string onHit, object[] hitParam,
        string beHurt, object[] hurtParam,
        string onKill, object[] killParam,
        string beKilled, object[] beKilledParam,
        ChaControlState stateMod, ChaProperty[] propMod = null
    )
    {
        this.id = id;
        this.name = name;
        this.tags = tags;
        this.priority = priority;
        this.maxStack = maxStack;
        this.stateMod = stateMod;
        this.tickTime = tickTime;

        // 初始化属性修改器数组 [0]=加值修改，[1]=乘值修改（百分比）
        this.propMod = new ChaProperty[2]{
            ChaProperty.zero,
            ChaProperty.zero
        };
        if (propMod != null)
        {
            for (int i = 0; i < Mathf.Min(2, propMod.Length); i++)
            {
                this.propMod[i] = propMod[i];
            }
        }

        // 从设计师脚本中查找并绑定回调函数
        this.onOccur = (onOccur == "") ? null : DesignerScripts.Buff.onOccurFunc[onOccur];
        this.onOccurParams = occurParam;
        this.onRemoved = (onRemoved == "") ? null : DesignerScripts.Buff.onRemovedFunc[onRemoved];
        this.onRemovedParams = removedParam;
        this.onTick = (onTick == "") ? null : DesignerScripts.Buff.onTickFunc[onTick];
        this.onTickParams = tickParam;
        this.onCast = (onCast == "") ? null : DesignerScripts.Buff.onCastFunc[onCast];
        this.onCastParams = castParam;
        this.onHit = (onHit == "") ? null : DesignerScripts.Buff.onHitFunc[onHit];
        this.onHitParams = hitParam;
        this.onBeHurt = (beHurt == "") ? null : DesignerScripts.Buff.beHurtFunc[beHurt];
        this.onBeHurtParams = hurtParam;
        this.onKill = (onKill == "") ? null : DesignerScripts.Buff.onKillFunc[onKill];
        this.onKillParams = killParam;
        this.onBeKilled = (beKilled == "") ? null : DesignerScripts.Buff.beKilledFunc[beKilled];
        this.onBeKilledParams = beKilledParam;
    }
}

/// <summary>
/// Buff应用时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="modifyStack">修改的层数</param>
public delegate void BuffOnOccur(BuffObj buff, int modifyStack);

/// <summary>
/// Buff移除时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
public delegate void BuffOnRemoved(BuffObj buff);

/// <summary>
/// Buff周期触发的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
public delegate void BuffOnTick(BuffObj buff);

/// <summary>
/// 角色造成伤害时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="damageInfo">伤害信息（可修改）</param>
/// <param name="target">伤害目标</param>
public delegate void BuffOnHit(BuffObj buff, ref DamageInfo damageInfo, GameObject target);

/// <summary>
/// 角色受到伤害时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="damageInfo">伤害信息（可修改）</param>
/// <param name="attacker">攻击者</param>
public delegate void BuffOnBeHurt(BuffObj buff, ref DamageInfo damageInfo, GameObject attacker);

/// <summary>
/// 角色击杀目标时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="damageInfo">伤害信息</param>
/// <param name="target">被击杀的目标</param>
public delegate void BuffOnKill(BuffObj buff, DamageInfo damageInfo, GameObject target);

/// <summary>
/// 角色被击杀时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="damageInfo">伤害信息</param>
/// <param name="attacker">击杀者</param>
public delegate void BuffOnBeKilled(BuffObj buff, DamageInfo damageInfo, GameObject attacker);

/// <summary>
/// 角色施放技能时的回调函数委托
/// </summary>
/// <param name="buff">Buff对象</param>
/// <param name="skill">技能对象</param>
/// <param name="timeline">时间线对象</param>
/// <returns>修改后的时间线对象</returns>
public delegate TimelineObj BuffOnCast(BuffObj buff, SkillObj skill, TimelineObj timeline);