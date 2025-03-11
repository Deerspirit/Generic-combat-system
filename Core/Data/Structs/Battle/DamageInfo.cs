using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伤害信息类
/// 封装一次伤害或治疗事件的所有相关信息
/// </summary>
public class DamageInfo : MonoBehaviour
{
    /// <summary>
    /// 攻击者对象
    /// </summary>
    public GameObject attacker;

    /// <summary>
    /// 防御者对象
    /// </summary>
    public GameObject defender;

    /// <summary>
    /// 伤害标签数组
    /// 定义伤害的类型和特性
    /// </summary>
    public DamageInfoTag[] tags;

    /// <summary>
    /// 伤害数值结构体
    /// 包含物理、法术和纯粹伤害
    /// </summary>
    public Damage damage;

    /// <summary>
    /// 暴击率
    /// 决定此次伤害是否为暴击
    /// </summary>
    public float criticalRate;

    /// <summary>
    /// 命中率
    /// 决定此次伤害是否命中
    /// </summary>
    public float hitRate = 1;

    /// <summary>
    /// 伤害方向角度
    /// 用于确定击退方向和特效朝向
    /// </summary>
    public float degree;

    /// <summary>
    /// 附加Buff列表
    /// 当伤害生效时会应用这些Buff
    /// </summary>
    public List<AddBuffInfo> addBuffs = new List<AddBuffInfo>();

    /// <summary>
    /// 创建伤害信息
    /// </summary>
    /// <param name="attacker">攻击者对象</param>
    /// <param name="defender">防御者对象</param>
    /// <param name="damage">伤害数值</param>
    /// <param name="damageDegree">伤害方向角度</param>
    /// <param name="baseCriticalRate">基础暴击率</param>
    /// <param name="tags">伤害标签数组</param>
    public DamageInfo(GameObject attacker, GameObject defender, Damage damage, float damageDegree, float baseCriticalRate, DamageInfoTag[] tags)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.damage = damage;
        this.criticalRate = baseCriticalRate;
        this.degree = damageDegree;
        this.tags = new DamageInfoTag[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            this.tags[i] = tags[i];
        }
    }

    /// <summary>
    /// 计算最终伤害值
    /// 考虑暴击、防御和其他修饰符
    /// </summary>
    /// <param name="asHeal">是否作为治疗计算</param>
    /// <returns>最终伤害或治疗数值</returns>
    public int DamageValue(bool asHeal)
    {
        return DesignerScripts.CommonScripts.DamageValue(this, asHeal);
    }

    /// <summary>
    /// 检查此伤害信息是否为治疗
    /// </summary>
    /// <returns>如果是治疗效果则返回true</returns>
    public bool isHeal()
    {
        for (int i = 0; i < this.tags.Length; i++)
        {
            if (tags[i] == DamageInfoTag.directHeal || tags[i] == DamageInfoTag.periodHeal)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查此伤害是否需要执行伤害逻辑
    /// </summary>
    /// <returns>如果需要执行伤害逻辑则返回true</returns>
    public bool requireDoHurt()
    {
        for (int i = 0; i < this.tags.Length; i++)
        {
            if (tags[i] == DamageInfoTag.directDamage)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 为此次伤害添加额外的Buff效果
    /// </summary>
    /// <param name="buffInfo">Buff添加信息</param>
    public void AddBuffToCha(AddBuffInfo buffInfo)
    {
        this.addBuffs.Add(buffInfo);
    }
}

/// <summary>
/// 伤害数值结构体
/// 封装不同类型的伤害数值
/// </summary>
public struct Damage
{
    /// <summary>
    /// 物理伤害值
    /// </summary>
    public int physical;
    
    /// <summary>
    /// 法术伤害值
    /// </summary>
    public int spell;
    
    /// <summary>
    /// 纯粹伤害值（无视防御）
    /// </summary>
    public int pure;

    /// <summary>
    /// 创建伤害数值
    /// </summary>
    /// <param name="physical">物理伤害值</param>
    /// <param name="spell">法术伤害值</param>
    /// <param name="pure">纯粹伤害值</param>
    public Damage(int physical, int spell = 0, int pure = 0)
    {
        this.physical = physical;
        this.spell = spell;
        this.pure = pure;
    }

    /// <summary>
    /// 计算总伤害值
    /// </summary>
    /// <param name="asHeal">是否作为治疗计算</param>
    /// <returns>总伤害或治疗数值</returns>
    public int Overall(bool asHeal = false)
    {
        return (asHeal == false) ?
            (Mathf.Max(0, physical) + Mathf.Max(0, spell) + Mathf.Max(0, pure)) :
            (Mathf.Min(0, physical) + Mathf.Min(0, spell) + Mathf.Min(0, pure));
    }

    /// <summary>
    /// 伤害相加运算符重载
    /// </summary>
    /// <param name="a">第一个伤害值</param>
    /// <param name="b">第二个伤害值</param>
    /// <returns>相加后的伤害值</returns>
    public static Damage operator +(Damage a, Damage b)
    {
        return new Damage(
            a.physical + b.physical,
            a.spell + b.spell,
            a.pure + b.pure
        );
    }

    /// <summary>
    /// 伤害乘以系数运算符重载
    /// </summary>
    /// <param name="a">伤害值</param>
    /// <param name="b">乘数系数</param>
    /// <returns>乘以系数后的伤害值</returns>
    public static Damage operator *(Damage a, float b)
    {
        return new Damage(
            Mathf.RoundToInt(a.physical * b),
            Mathf.RoundToInt(a.spell * b),
            Mathf.RoundToInt(a.pure * b)
        );
    }
}

/// <summary>
/// 伤害信息标签枚举
/// 定义伤害的类型和特性
/// </summary>
public enum DamageInfoTag
{
    /// <summary>
    /// 直接伤害（普通攻击、技能）
    /// </summary>
    directDamage = 0,
    
    /// <summary>
    /// 周期伤害（持续伤害、毒药）
    /// </summary>
    periodDamage = 1,
    
    /// <summary>
    /// 反射伤害（荆棘、反伤）
    /// </summary>
    reflectDamage = 2,
    
    /// <summary>
    /// 直接治疗（治疗技能）
    /// </summary>
    directHeal = 10,
    
    /// <summary>
    /// 周期治疗（持续恢复）
    /// </summary>
    periodHeal = 11,
    
    /// <summary>
    /// 特殊伤害类型（猴子伤害）
    /// </summary>
    monkeyDamage = 9999
}
