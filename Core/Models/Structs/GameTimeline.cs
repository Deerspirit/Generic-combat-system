using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 时间线对象类
/// 表示一个正在执行的时间线实例，包含执行状态和上下文信息
/// </summary>
public class TimelineObj
{
    /// <summary>
    /// 时间线模型，定义了时间线的结构和行为
    /// </summary>
    public TimelineModel model;

    /// <summary>
    /// 施放者对象，即触发此时间线的角色
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// 时间线执行速度缩放
    /// 控制时间线的执行速度，最小值为0.1
    /// </summary>
    public float timeScale
    {
        get
        {
            return _timeScale;
        }
        set
        {
            _timeScale = Mathf.Max(0.100f, value);
        }
    }
    
    /// <summary>
    /// 时间线速度缩放的内部存储字段
    /// </summary>
    private float _timeScale = 1.00f;

    /// <summary>
    /// 时间线自定义参数
    /// 可用于在时间线执行过程中传递和访问数据
    /// </summary>
    public object param;

    /// <summary>
    /// 时间线已经执行的时间（秒）
    /// </summary>
    public float timeElapsed = 0;

    /// <summary>
    /// 时间线上下文值字典
    /// 存储时间线执行过程中的各种状态值
    /// </summary>
    public Dictionary<string, object> values;

    /// <summary>
    /// 创建时间线对象
    /// </summary>
    /// <param name="model">时间线模型</param>
    /// <param name="caster">施放者对象</param>
    /// <param name="param">自定义参数</param>
    public TimelineObj(TimelineModel model, GameObject caster, object param)
    {
        this.model = model;
        this.caster = caster;
        this.values = new Dictionary<string, object>();
        this._timeScale = 1.00f;
        
        // 如果有施放者，初始化面向角度和移动角度，并设置执行速度
        if (caster)
        {
            ChaState characterState = caster.GetComponent<ChaState>();
            if (characterState)
            {
                this.values.Add("faceDegree", characterState.faceDegree);
                this.values.Add("moveDegree", characterState.moveDegree);
            }
            // 使用角色的行动速度作为时间线速度
            this._timeScale = characterState.actionSpeed;
        }
        
        this.param = param;
    }

    /// <summary>
    /// 获取时间线上下文中的值
    /// </summary>
    /// <param name="key">值的键名</param>
    /// <returns>对应键的值，若不存在则返回null</returns>
    public object GetValue(string key)
    {
        if (values.ContainsKey(key) == false)
            return null;
        return values[key];
    }
}

/// <summary>
/// 时间线模型结构体
/// 定义时间线的结构、节点和行为
/// </summary>
public struct TimelineModel
{
    /// <summary>
    /// 时间线唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// 时间线节点数组
    /// 定义时间线上的所有事件节点
    /// </summary>
    public TimelineNode[] nodes;

    /// <summary>
    /// 时间线总持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// 蓄力返回点
    /// 用于技能蓄力时的时间线跳转
    /// </summary>
    public TimelineGoTo chargeGoBack;

    /// <summary>
    /// 创建时间线模型
    /// </summary>
    /// <param name="id">唯一标识符</param>
    /// <param name="nodes">节点数组</param>
    /// <param name="duration">总持续时间</param>
    /// <param name="chargeGoBack">蓄力返回点</param>
    public TimelineModel(string id, TimelineNode[] nodes, float duration, TimelineGoTo chargeGoBack)
    {
        this.id = id;
        this.nodes = nodes;
        this.duration = duration;
        this.chargeGoBack = chargeGoBack;
    }
}

/// <summary>
/// 时间线节点结构体
/// 定义时间线上的单个事件点
/// </summary>
public struct TimelineNode
{
    /// <summary>
    /// 节点触发时间（秒）
    /// 相对于时间线开始的时间
    /// </summary>
    public float timeElapsed;

    /// <summary>
    /// 节点执行的事件函数
    /// </summary>
    public TimelineEvent doEvent;

    /// <summary>
    /// 事件函数的参数数组
    /// </summary>
    public object[] eveParams { get; }

    /// <summary>
    /// 创建时间线节点
    /// </summary>
    /// <param name="time">触发时间</param>
    /// <param name="doEve">事件函数名称</param>
    /// <param name="eveArgs">事件函数参数</param>
    public TimelineNode(float time, string doEve, params object[] eveArgs)
    {
        this.timeElapsed = time;
        this.doEvent = DesignerScripts.Timeline.functions[doEve];
        this.eveParams = eveArgs;
    }
}

/// <summary>
/// 时间线跳转点结构体
/// 定义时间线的跳转目标和条件
/// </summary>
public struct TimelineGoTo
{
    /// <summary>
    /// 触发跳转的时间点（秒）
    /// </summary>
    public float atDuration;

    /// <summary>
    /// 跳转目标时间点（秒）
    /// </summary>
    public float gotoDuration;

    /// <summary>
    /// 创建时间线跳转点
    /// </summary>
    /// <param name="atDuration">触发跳转的时间点</param>
    /// <param name="gotoDuration">跳转目标时间点</param>
    public TimelineGoTo(float atDuration, float gotoDuration)
    {
        this.atDuration = atDuration;
        this.gotoDuration = gotoDuration;
    }

    /// <summary>
    /// 空跳转点
    /// 表示不会触发的跳转点
    /// </summary>
    public static TimelineGoTo Null = new TimelineGoTo(float.MaxValue, float.MaxValue);
}

/// <summary>
/// 时间线事件委托
/// 定义时间线节点可执行的事件函数签名
/// </summary>
/// <param name="timeline">时间线对象</param>
/// <param name="args">事件参数</param>
public delegate void TimelineEvent(TimelineObj timeline, params object[] args);
