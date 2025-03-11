using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AOE发射器类
/// 封装创建一个范围效果所需的所有属性和行为
/// </summary>
public class AoeLauncher
{
    /// <summary>
    /// AOE模型，定义AOE的基本属性和回调函数
    /// </summary>
    public AoeModel model;

    /// <summary>
    /// AOE在世界空间中的位置
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// AOE的施放者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// AOE的影响半径
    /// </summary>
    public float radius;

    /// <summary>
    /// AOE的持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// AOE的朝向角度
    /// </summary>
    public float degree;

    /// <summary>
    /// AOE的移动函数
    /// </summary>
    public AoeTween tween;

    /// <summary>
    /// AOE移动函数的参数数组
    /// </summary>
    public object[] tweenParam = new object[0];

    /// <summary>
    /// AOE自定义参数字典
    /// </summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    /// <summary>
    /// 创建一个AOE发射器
    /// </summary>
    /// <param name="model">AOE模型</param>
    /// <param name="caster">施放者</param>
    /// <param name="position">位置</param>
    /// <param name="radius">半径</param>
    /// <param name="duration">持续时间</param>
    /// <param name="degree">朝向角度</param>
    /// <param name="tween">移动函数</param>
    /// <param name="tweenParam">移动函数参数</param>
    /// <param name="aoeParam">自定义参数字典</param>
    public AoeLauncher(
        AoeModel model, GameObject caster, Vector3 position, float radius, float duration, float degree,
        AoeTween tween = null, object[] tweenParam = null, Dictionary<string, object> aoeParam = null
    )
    {
        this.model = model;
        this.caster = caster;
        this.position = position;
        this.radius = radius;
        this.duration = duration;
        this.degree = degree;
        this.tween = tween;
        if (aoeParam != null) this.param = aoeParam;
        if (tweenParam != null) this.tweenParam = tweenParam;
    }

    /// <summary>
    /// 克隆当前AOE发射器
    /// 创建一个具有相同属性的新发射器对象
    /// </summary>
    /// <returns>克隆的AOE发射器</returns>
    public AoeLauncher Clone()
    {
        return new AoeLauncher(
            this.model,
            this.caster,
            this.position,
            this.radius,
            this.duration,
            this.degree,
            this.tween,
            this.tweenParam,
            this.param
        );
    }
}

/// <summary>
/// AOE模型结构体
/// 定义AOE的基本属性和行为
/// </summary>
public struct AoeModel
{
    /// <summary>
    /// AOE唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// AOE预制体路径
    /// </summary>
    public string prefab;

    /// <summary>
    /// 是否在碰到障碍物时移除
    /// </summary>
    public bool removeOnObstacle;

    /// <summary>
    /// AOE标签数组
    /// </summary>
    public string[] tags;

    /// <summary>
    /// AOE周期触发时间间隔（秒）
    /// </summary>
    public float tickTime;

    /// <summary>
    /// AOE创建时回调函数
    /// </summary>
    public AoeOnCreate onCreate;

    /// <summary>
    /// AOE创建时回调函数参数
    /// </summary>
    public object[] onCreateParams;

    /// <summary>
    /// AOE周期触发回调函数
    /// </summary>
    public AoeOnTick onTick;

    /// <summary>
    /// AOE周期触发回调函数参数
    /// </summary>
    public object[] onTickParams;

    /// <summary>
    /// AOE移除时回调函数
    /// </summary>
    public AoeOnRemoved onRemoved;

    /// <summary>
    /// AOE移除时回调函数参数
    /// </summary>
    public object[] onRemovedParams;

    /// <summary>
    /// 角色进入AOE区域回调函数
    /// </summary>
    public AoeOnCharacterEnter onChaEnter;

    /// <summary>
    /// 角色进入AOE区域回调函数参数
    /// </summary>
    public object[] onChaEnterParams;

    /// <summary>
    /// 角色离开AOE区域回调函数
    /// </summary>
    public AoeOnCharacterLeave onChaLeave;

    /// <summary>
    /// 角色离开AOE区域回调函数参数
    /// </summary>
    public object[] onChaLeaveParams;

    /// <summary>
    /// 子弹进入AOE区域回调函数
    /// </summary>
    public AoeOnBulletEnter onBulletEnter;

    /// <summary>
    /// 子弹进入AOE区域回调函数参数
    /// </summary>
    public object[] onBulletEnterParams;

    /// <summary>
    /// 子弹离开AOE区域回调函数
    /// </summary>
    public AoeOnBulletLeave onBulletLeave;

    /// <summary>
    /// 子弹离开AOE区域回调函数参数
    /// </summary>
    public object[] onBulletLeaveParams;

    /// <summary>
    /// 创建AOE模型
    /// </summary>
    /// <param name="id">唯一标识符</param>
    /// <param name="prefab">预制体路径</param>
    /// <param name="tags">标签数组</param>
    /// <param name="tickTime">周期触发间隔</param>
    /// <param name="removeOnObstacle">是否碰障碍物移除</param>
    /// <param name="onCreate">创建时回调函数名</param>
    /// <param name="onCreateParam">创建时回调函数参数</param>
    /// <param name="onRemoved">移除时回调函数名</param>
    /// <param name="onRemovedParam">移除时回调函数参数</param>
    /// <param name="onTick">周期触发回调函数名</param>
    /// <param name="onTickParam">周期触发回调函数参数</param>
    /// <param name="onChaEnter">角色进入回调函数名</param>
    /// <param name="onChaEnterParam">角色进入回调函数参数</param>
    /// <param name="onChaLeave">角色离开回调函数名</param>
    /// <param name="onChaLeaveParam">角色离开回调函数参数</param>
    /// <param name="onBulletEnter">子弹进入回调函数名</param>
    /// <param name="onBulletEnterParam">子弹进入回调函数参数</param>
    /// <param name="onBulletLeave">子弹离开回调函数名</param>
    /// <param name="onBulletLeaveParam">子弹离开回调函数参数</param>
    public AoeModel(
        string id, string prefab, string[] tags, float tickTime, bool removeOnObstacle,
        string onCreate, object[] onCreateParam,
        string onRemoved, object[] onRemovedParam,
        string onTick, object[] onTickParam,
        string onChaEnter, object[] onChaEnterParam,
        string onChaLeave, object[] onChaLeaveParam,
        string onBulletEnter, object[] onBulletEnterParam,
        string onBulletLeave, object[] onBulletLeaveParam
    )
    {
        this.id = id;
        this.prefab = prefab;
        this.tags = tags;
        this.tickTime = tickTime;
        this.removeOnObstacle = removeOnObstacle;

        // 从设计师脚本中获取回调函数
        this.onCreate = onCreate == "" ? null : DesignerScripts.AoE.onCreateFunc[onCreate];
        this.onCreateParams = onCreateParam;
        this.onRemoved = onRemoved == "" ? null : DesignerScripts.AoE.onRemovedFunc[onRemoved];
        this.onRemovedParams = onRemovedParam;
        this.onTick = onTick == "" ? null : DesignerScripts.AoE.onTickFunc[onTick];
        this.onTickParams = onTickParam;
        this.onChaEnter = onChaEnter == "" ? null : DesignerScripts.AoE.onChaEnterFunc[onChaEnter];
        this.onChaEnterParams = onChaEnterParam;
        this.onChaLeave = onChaLeave == "" ? null : DesignerScripts.AoE.onChaLeaveFunc[onChaLeave];
        this.onChaLeaveParams = onChaLeaveParam;
        this.onBulletEnter = onBulletEnter == "" ? null : DesignerScripts.AoE.onBulletEnterFunc[onBulletEnter];
        this.onBulletEnterParams = onBulletEnterParam;
        this.onBulletLeave = onBulletLeave == "" ? null : DesignerScripts.AoE.onBulletLeaveFunc[onBulletLeave];
        this.onBulletLeaveParams = onBulletLeaveParam;
    }
}

/// <summary>
/// AOE移动信息类
/// 封装AOE的移动类型、速度和旋转信息
/// </summary>
public class AoeMoveInfo
{
    /// <summary>
    /// AOE的移动类型
    /// </summary>
    public MoveType moveType;

    /// <summary>
    /// AOE的移动速度向量
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// AOE要旋转到的角度
    /// </summary>
    public float rotateToDegree;

    /// <summary>
    /// 创建AOE移动信息
    /// </summary>
    /// <param name="moveType">移动类型</param>
    /// <param name="velocity">速度向量</param>
    /// <param name="rotateToDegree">旋转角度</param>
    public AoeMoveInfo(MoveType moveType, Vector3 velocity, float rotateToDegree)
    {
        this.moveType = moveType;
        this.velocity = velocity;
        this.rotateToDegree = rotateToDegree;
    }
}

/// <summary>
/// AOE创建时回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
public delegate void AoeOnCreate(GameObject aoe);

/// <summary>
/// AOE移除时回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
public delegate void AoeOnRemoved(GameObject aoe);

/// <summary>
/// AOE周期触发回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
public delegate void AoeOnTick(GameObject aoe);

/// <summary>
/// 角色进入AOE区域回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
/// <param name="cha">进入区域的角色列表</param>
public delegate void AoeOnCharacterEnter(GameObject aoe, List<GameObject> cha);

/// <summary>
/// 角色离开AOE区域回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
/// <param name="cha">离开区域的角色列表</param>
public delegate void AoeOnCharacterLeave(GameObject aoe, List<GameObject> cha);

/// <summary>
/// 子弹进入AOE区域回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
/// <param name="bullet">进入区域的子弹列表</param>
public delegate void AoeOnBulletEnter(GameObject aoe, List<GameObject> bullet);

/// <summary>
/// 子弹离开AOE区域回调函数委托
/// </summary>
/// <param name="aoe">AOE对象</param>
/// <param name="bullet">离开区域的子弹列表</param>
public delegate void AoeOnBulletLeave(GameObject aoe, List<GameObject> bullet);

/// <summary>
/// AOE移动轨迹计算委托
/// </summary>
/// <param name="aoe">AOE对象</param>
/// <param name="t">时间参数（0到1之间）</param>
/// <returns>AOE移动信息</returns>
public delegate AoeMoveInfo AoeTween(GameObject aoe, float t);
