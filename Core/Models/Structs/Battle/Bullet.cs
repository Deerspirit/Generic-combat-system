using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹发射器类
/// 封装创建一个子弹所需的所有属性和行为
/// </summary>
public class BulletLauncher
{
    /// <summary>
    /// 子弹模型，定义子弹的基本属性和回调函数
    /// </summary>
    public BulletModel model;

    /// <summary>
    /// 子弹的施放者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// 子弹发射的起始位置
    /// </summary>
    public Vector3 firePosition;

    /// <summary>
    /// 子弹发射的角度
    /// </summary>
    public float fireDegree;

    /// <summary>
    /// 子弹移动速度
    /// </summary>
    public float speed;

    /// <summary>
    /// 子弹持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// 子弹目标获取函数
    /// </summary>
    public BulletTargettingFunction targetFunc;

    /// <summary>
    /// 子弹移动轨迹函数
    /// </summary>
    public BulletTween tween = null;

    /// <summary>
    /// 是否永远使用初始发射角度
    /// </summary>
    public bool useFireDegreeForever = false;

    /// <summary>
    /// 子弹创建后多久可以命中目标（秒）
    /// </summary>
    public float canHitAfterCreated = 0;

    /// <summary>
    /// 子弹自定义参数字典
    /// </summary>
    public Dictionary<string, object> param;

    /// <summary>
    /// 创建一个子弹发射器
    /// </summary>
    /// <param name="model">子弹模型</param>
    /// <param name="caster">施放者</param>
    /// <param name="firePos">发射位置</param>
    /// <param name="degree">发射角度</param>
    /// <param name="speed">移动速度</param>
    /// <param name="duration">持续时间</param>
    /// <param name="canHitAfterCreated">创建后多久可以命中目标</param>
    /// <param name="tween">移动轨迹函数</param>
    /// <param name="targetFunc">目标获取函数</param>
    /// <param name="useFireDegree">是否永远使用初始发射角度</param>
    /// <param name="param">自定义参数字典</param>
    public BulletLauncher(
        BulletModel model, GameObject caster, Vector3 firePos, float degree, float speed, float duration,
        float canHitAfterCreated = 0,
        BulletTween tween = null, BulletTargettingFunction targetFunc = null, bool useFireDegree = false,
        Dictionary<string, object> param = null)
    {
        this.model = model;
        this.caster = caster;
        this.firePosition = firePos;
        this.fireDegree = degree;
        this.speed = speed;
        this.duration = duration;
        this.tween = tween;
        this.targetFunc = targetFunc;
        this.useFireDegreeForever = useFireDegree;
        this.canHitAfterCreated = canHitAfterCreated;
        this.param = param;
    }
}

/// <summary>
/// 子弹模型结构体
/// 定义子弹的基本属性和行为
/// </summary>
public struct BulletModel
{
    /// <summary>
    /// 子弹唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// 子弹预制体路径
    /// </summary>
    public string prefab;

    /// <summary>
    /// 子弹碰撞半径
    /// </summary>
    public float radius;

    /// <summary>
    /// 子弹可命中次数
    /// </summary>
    public int hitTimes;

    /// <summary>
    /// 命中同一目标的间隔时间（秒）
    /// </summary>
    public float sameTargetDelay;

    /// <summary>
    /// 子弹创建时回调函数
    /// </summary>
    public BulletOnCreate onCreate;

    /// <summary>
    /// 子弹创建时回调函数参数
    /// </summary>
    public object[] onCreateParam;

    /// <summary>
    /// 子弹命中目标时回调函数
    /// </summary>
    public BulletOnHit onHit;

    /// <summary>
    /// 子弹命中目标时回调函数参数
    /// </summary>
    public object[] onHitParams;

    /// <summary>
    /// 子弹移除时回调函数
    /// </summary>
    public BulletOnRemoved onRemoved;

    /// <summary>
    /// 子弹移除时回调函数参数
    /// </summary>
    public object[] onRemovedParams;

    /// <summary>
    /// 子弹移动类型
    /// </summary>
    public MoveType moveType;

    /// <summary>
    /// 是否在碰到障碍物时移除
    /// </summary>
    public bool removeOnObstacle;

    /// <summary>
    /// 是否可命中敌方目标
    /// </summary>
    public bool hitFoe;

    /// <summary>
    /// 是否可命中友方目标
    /// </summary>
    public bool hitAlly;

    /// <summary>
    /// 创建子弹模型
    /// </summary>
    /// <param name="id">唯一标识符</param>
    /// <param name="prefab">预制体路径</param>
    /// <param name="onCreate">创建时回调函数名</param>
    /// <param name="createParams">创建时回调函数参数</param>
    /// <param name="onHit">命中时回调函数名</param>
    /// <param name="onHitParams">命中时回调函数参数</param>
    /// <param name="onRemoved">移除时回调函数名</param>
    /// <param name="onRemovedParams">移除时回调函数参数</param>
    /// <param name="moveType">移动类型</param>
    /// <param name="removeOnObstacle">是否碰障碍物移除</param>
    /// <param name="radius">碰撞半径</param>
    /// <param name="hitTimes">可命中次数</param>
    /// <param name="sameTargetDelay">命中同一目标的间隔</param>
    /// <param name="hitFoe">是否可命中敌方</param>
    /// <param name="hitAlly">是否可命中友方</param>
    public BulletModel(
        string id, string prefab,
        string onCreate = "",
        object[] createParams = null,
        string onHit = "",
        object[] onHitParams = null,
        string onRemoved = "",
        object[] onRemovedParams = null,
        MoveType moveType = MoveType.fly, bool removeOnObstacle = true,
        float radius = 0.1f, int hitTimes = 1, float sameTargetDelay = 0.1f,
        bool hitFoe = true, bool hitAlly = false)
    {
        this.id = id;
        this.prefab = prefab;
        
        // 从设计师脚本中获取回调函数
        this.onHit = onHit == "" ? null : DesignerScripts.Bullet.onHitFunc[onHit];
        this.onRemoved = onRemoved == "" ? null : DesignerScripts.Bullet.onRemovedFunc[onRemoved];
        this.onCreate = onCreate == "" ? null : DesignerScripts.Bullet.onCreateFunc[onCreate];
        
        // 初始化回调函数参数
        this.onCreateParam = createParams != null ? createParams : new object[0];
        this.onHitParams = onHitParams != null ? onHitParams : new object[0];
        this.onRemovedParams = onRemovedParams != null ? onRemovedParams : new object[0];
        
        // 设置物理和碰撞属性
        this.radius = radius;
        this.hitTimes = hitTimes;
        this.sameTargetDelay = sameTargetDelay;
        this.moveType = moveType;
        this.removeOnObstacle = removeOnObstacle;
        
        // 设置命中规则
        this.hitAlly = hitAlly;
        this.hitFoe = hitFoe;
    }
}

/// <summary>
/// 子弹命中记录类
/// 用于跟踪子弹命中目标的状态和冷却时间
/// </summary>
public class BulletHitRecord
{
    /// <summary>
    /// 被命中的目标
    /// </summary>
    public GameObject target;

    /// <summary>
    /// 下次可以命中该目标的时间点
    /// </summary>
    public float timeToCanHit;

    /// <summary>
    /// 创建子弹命中记录
    /// </summary>
    /// <param name="character">被命中的角色对象</param>
    /// <param name="timeToCanHit">下次可以命中的时间点</param>
    public BulletHitRecord(GameObject character, float timeToCanHit)
    {
        this.target = character;
        this.timeToCanHit = timeToCanHit;
    }
}

/// <summary>
/// 子弹创建时回调函数委托
/// </summary>
/// <param name="bullet">子弹对象</param>
public delegate void BulletOnCreate(GameObject bullet);

/// <summary>
/// 子弹命中目标时回调函数委托
/// </summary>
/// <param name="bullet">子弹对象</param>
/// <param name="target">命中的目标</param>
public delegate void BulletOnHit(GameObject bullet, GameObject target);

/// <summary>
/// 子弹移除时回调函数委托
/// </summary>
/// <param name="bullet">子弹对象</param>
public delegate void BulletOnRemoved(GameObject bullet);

/// <summary>
/// 子弹移动轨迹计算委托
/// </summary>
/// <param name="t">时间参数（0到1之间）</param>
/// <param name="bullet">子弹对象</param>
/// <param name="target">目标对象</param>
/// <returns>移动方向向量</returns>
public delegate Vector3 BulletTween(float t, GameObject bullet, GameObject target);

/// <summary>
/// 子弹目标获取函数委托
/// </summary>
/// <param name="bullet">子弹对象</param>
/// <param name="targets">可能的目标数组</param>
/// <returns>选择的目标对象</returns>
public delegate GameObject BulletTargettingFunction(GameObject bullet, GameObject[] targets);
