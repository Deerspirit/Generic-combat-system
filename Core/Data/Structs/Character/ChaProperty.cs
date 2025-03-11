using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色属性结构体：定义角色的基础和战斗属性
/// 包括生命值、攻击力、移动速度、行动速度、弹药量和碰撞体积等
/// </summary>
public struct ChaProperty
{
    /// <summary>
    /// 生命值上限
    /// </summary>
    public int hp;

    /// <summary>
    /// 攻击力，影响角色技能和武器伤害
    /// </summary>
    public int attack;

    /// <summary>
    /// 移动速度，影响角色的移动快慢
    /// </summary>
    public int moveSpeed;

    /// <summary>
    /// 行动速度，影响角色的攻击和技能释放速度
    /// </summary>
    public int actionSpeed;

    /// <summary>
    /// 弹药量，影响某些技能的使用次数
    /// </summary>
    public int ammo;

    /// <summary>
    /// 物理碰撞体半径，用于碰撞检测
    /// </summary>
    public float bodyRadius;

    /// <summary>
    /// 受击判定半径，用于伤害检测
    /// </summary>
    public float hitRadius;

    /// <summary>
    /// 移动类型，决定角色是在地面上移动还是飞行
    /// </summary>
    public MoveType moveType;

    /// <summary>
    /// 创建角色属性实例
    /// </summary>
    /// <param name="moveSpeed">移动速度</param>
    /// <param name="hp">生命值上限</param>
    /// <param name="ammo">弹药量</param>
    /// <param name="attack">攻击力</param>
    /// <param name="actionSpeed">行动速度</param>
    /// <param name="bodyRadius">物理碰撞体半径</param>
    /// <param name="hitRadius">受击判定半径</param>
    /// <param name="moveType">移动类型</param>
    public ChaProperty(int moveSpeed, int hp = 0, int ammo = 0, int attack = 0, int actionSpeed = 100, float bodyRadius = 0.25f, float hitRadius = 0.25f, MoveType moveType = MoveType.ground)
    {
        this.hp = hp;
        this.attack = attack;
        this.moveSpeed = moveSpeed;
        this.actionSpeed = actionSpeed;
        this.ammo = ammo;
        this.bodyRadius = bodyRadius;
        this.hitRadius = hitRadius;
        this.moveType = moveType;
    }

    /// <summary>
    /// 零属性常量，所有数值都为0
    /// </summary>
    public static ChaProperty zero = new ChaProperty(0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// 将当前属性全部设置为0
    /// </summary>
    /// <param name="moveType">移动类型，默认为地面</param>
    public void Zero(MoveType moveType = MoveType.ground)
    {
        this.hp = 0;
        this.attack = 0;
        this.moveSpeed = 0;
        this.actionSpeed = 0;
        this.ammo = 0;
        this.bodyRadius = 0;
        this.hitRadius = 0;
        this.moveType = moveType;
    }

    /// <summary>
    /// 运算符重载：属性相加
    /// 所有数值属性直接相加，移动类型如果任一为飞行则结果为飞行
    /// </summary>
    /// <param name="a">第一个属性</param>
    /// <param name="b">第二个属性</param>
    /// <returns>相加后的新属性</returns>
    public static ChaProperty operator +(ChaProperty a, ChaProperty b)
    {
        return new ChaProperty(
            moveSpeed: a.moveSpeed + b.moveSpeed,
            hp: a.hp + b.hp,
            ammo: a.ammo + b.ammo,
            attack: a.attack + b.attack,
            actionSpeed: a.actionSpeed + b.actionSpeed,
            bodyRadius: a.bodyRadius + b.bodyRadius,
            hitRadius: a.hitRadius + b.hitRadius,
            moveType: a.moveType == MoveType.fly || b.moveType == MoveType.fly ? MoveType.fly : MoveType.ground
        );
    }

    /// <summary>
    /// 运算符重载：属性相乘（百分比加成）
    /// b的属性值作为a的百分比增益，如b.hp=0.5表示增加50%的生命值
    /// 同时防止负值过大导致属性为负（限制最小为-99.99%）
    /// </summary>
    /// <param name="a">基础属性</param>
    /// <param name="b">百分比加成属性</param>
    /// <returns>计算后的新属性</returns>
    public static ChaProperty operator *(ChaProperty a, ChaProperty b)
    {
        return new ChaProperty(
            moveSpeed: Mathf.RoundToInt(a.moveSpeed * (1.0000f + Mathf.Max(b.moveSpeed, -0.9999f))),
            hp: Mathf.RoundToInt(a.hp * (1.0000f + Mathf.Max(b.hp, -0.9999f))),
            ammo: Mathf.RoundToInt(a.ammo * (1.0000f + Mathf.Max(b.ammo, -0.9999f))),
            attack: Mathf.RoundToInt(a.attack * (1.0000f + Mathf.Max(b.attack, -0.9999f))),
            actionSpeed: Mathf.RoundToInt(a.actionSpeed * (1.0000f + Mathf.Max(b.actionSpeed, -0.9999f))),
            bodyRadius: a.bodyRadius * (1.0000f + Mathf.Max(b.bodyRadius, -0.9999f)),
            hitRadius: a.hitRadius * (1.0000f + Mathf.Max(b.hitRadius, -0.9999f)),
            moveType: a.moveType == MoveType.fly || b.moveType == MoveType.fly ? MoveType.fly : MoveType.ground
        );
    }

    /// <summary>
    /// 运算符重载：属性乘以系数
    /// 所有数值属性都乘以同一个系数
    /// </summary>
    /// <param name="a">属性</param>
    /// <param name="b">系数</param>
    /// <returns>计算后的新属性</returns>
    public static ChaProperty operator *(ChaProperty a, float b)
    {
        return new ChaProperty(
            Mathf.RoundToInt(a.moveSpeed * b),
            Mathf.RoundToInt(a.hp * b),
            Mathf.RoundToInt(a.ammo * b),
            Mathf.RoundToInt(a.attack * b),
            Mathf.RoundToInt(a.actionSpeed * b),
            a.bodyRadius * b,
            a.hitRadius * b,
            a.moveType
        );
    }
}

/// <summary>
/// 角色资源类：表示角色当前拥有的可消耗资源
/// 包括当前生命值、弹药量和耐力值
/// </summary>
public class ChaResource
{
    /// <summary>
    /// 当前生命值
    /// </summary>
    public int hp;

    /// <summary>
    /// 当前弹药量
    /// </summary>
    public int ammo;

    /// <summary>
    /// 当前耐力值
    /// </summary>
    public int stamina;

    /// <summary>
    /// 创建角色资源实例
    /// </summary>
    /// <param name="hp">当前生命值</param>
    /// <param name="ammo">当前弹药量</param>
    /// <param name="stamina">当前耐力值</param>
    public ChaResource(int hp, int ammo = 0, int stamina = 0)
    {
        this.hp = hp;
        this.ammo = ammo;
        this.stamina = stamina;
    }

    /// <summary>
    /// 检查当前资源是否满足需求
    /// </summary>
    /// <param name="requirement">需求资源</param>
    /// <returns>如果当前资源大于或等于需求资源则返回true</returns>
    public bool Enough(ChaResource requirement)
    {
        return (
            this.hp >= requirement.hp &&
            this.ammo >= requirement.ammo &&
            this.stamina >= requirement.stamina
        );
    }

    /// <summary>
    /// 运算符重载：资源相加
    /// </summary>
    /// <param name="a">第一个资源</param>
    /// <param name="b">第二个资源</param>
    /// <returns>相加后的新资源</returns>
    public static ChaResource operator +(ChaResource a, ChaResource b)
    {
        return new ChaResource(
            a.hp + b.hp,
            a.ammo + b.ammo,
            a.stamina + b.stamina
        );
    }

    /// <summary>
    /// 运算符重载：资源乘以系数
    /// </summary>
    /// <param name="a">资源</param>
    /// <param name="b">系数</param>
    /// <returns>计算后的新资源</returns>
    public static ChaResource operator *(ChaResource a, float b)
    {
        return new ChaResource(
             Mathf.RoundToInt(a.hp * b),
             Mathf.RoundToInt(a.ammo * b),
             Mathf.RoundToInt(a.stamina * b)
        );
    }

    /// <summary>
    /// 运算符重载：系数乘以资源
    /// </summary>
    /// <param name="a">系数</param>
    /// <param name="b">资源</param>
    /// <returns>计算后的新资源</returns>
    public static ChaResource operator *(float a, ChaResource b)
    {
        return new ChaResource(
            Mathf.RoundToInt(a * b.hp),
            Mathf.RoundToInt(a * b.ammo),
            Mathf.RoundToInt(a * b.stamina)
        );
    }

    /// <summary>
    /// 运算符重载：资源相减
    /// </summary>
    /// <param name="a">被减资源</param>
    /// <param name="b">减去的资源</param>
    /// <returns>相减后的新资源</returns>
    public static ChaResource operator -(ChaResource a, ChaResource b)
    {
        return a + b * -1;
    }

    /// <summary>
    /// 空资源常量，所有资源值都为0
    /// </summary>
    public static ChaResource Null = new ChaResource(0);
}
