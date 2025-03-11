using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备对象：表示角色当前装备的具体物品实例
/// 包含装备模型的引用
/// </summary>
public struct EquipmentObj
{
    /// <summary>
    /// 装备模型引用
    /// </summary>
    public EquipmentModel model;

    /// <summary>
    /// 创建装备对象实例
    /// </summary>
    /// <param name="model">装备模型</param>
    public EquipmentObj(EquipmentModel model)
    {
        this.model = model;
    }
}

/// <summary>
/// 装备模型：定义装备的基本属性和效果
/// 包括装备ID、图标、名称、标签、属性加成、技能和Buff
/// </summary>
public struct EquipmentModel
{
    /// <summary>
    /// 装备唯一标识符
    /// </summary>
    public string id;

    /// <summary>
    /// 装备图标资源路径
    /// </summary>
    public string icon;

    /// <summary>
    /// 装备显示名称
    /// </summary>
    public string name;

    /// <summary>
    /// 装备标签数组，用于分类和过滤
    /// </summary>
    public string[] tags;

    /// <summary>
    /// 装备槽位类型
    /// </summary>
    public EquipmentSlot slot;

    /// <summary>
    /// 装备提供的属性加成
    /// </summary>
    public ChaProperty equipmentProperty;

    /// <summary>
    /// 装备提供的技能数组
    /// </summary>
    public SkillModel[] skills;

    /// <summary>
    /// 装备提供的Buff数组
    /// </summary>
    public AddBuffInfo[] buffs;

    /// <summary>
    /// 创建装备模型实例
    /// </summary>
    /// <param name="id">装备唯一标识符</param>
    /// <param name="icon">装备图标资源路径</param>
    /// <param name="name">装备显示名称</param>
    /// <param name="tags">装备标签数组</param>
    /// <param name="equipment">装备提供的属性加成</param>
    /// <param name="skills">装备提供的技能数组</param>
    /// <param name="buffs">装备提供的Buff数组</param>
    /// <param name="slot">装备槽位类型，默认为武器</param>
    public EquipmentModel(string id, string icon, string name, string[] tags,
        ChaProperty equipment,
        SkillModel[] skills,
        AddBuffInfo[] buffs,
        EquipmentSlot slot = EquipmentSlot.weapon)
    {
        this.id = id;
        this.icon = icon;
        this.name = name;
        this.tags = tags;
        this.slot = slot;
        this.equipmentProperty = equipment;
        this.skills = skills;
        this.buffs = buffs;
    }
}

/// <summary>
/// 装备槽位枚举：定义装备可以被装备的位置
/// </summary>
public enum EquipmentSlot
{
    /// <summary>
    /// 武器槽位
    /// </summary>
    weapon = 1,
    
    /// <summary>
    /// 头盔槽位
    /// </summary>
    helm = 2,
    
    /// <summary>
    /// 护甲槽位
    /// </summary>
    armor = 3,
    
    /// <summary>
    /// 饰品槽位
    /// </summary>
    trinket = 4
}
