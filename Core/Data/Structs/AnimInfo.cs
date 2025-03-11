using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画信息类
/// 用于管理带权重的动画集合，支持随机选择和查询功能
/// </summary>
public class AnimInfo
{
    /// <summary>
    /// 动画集合的唯一标识符
    /// </summary>
    public string key;

    /// <summary>
    /// 动画集合的优先级
    /// 数值越高优先级越高
    /// </summary>
    public int priority;

    /// <summary>
    /// 动画数组，每个元素包含动画信息和权重
    /// Key: 单个动画信息，Value: 权重值
    /// </summary>
    public KeyValuePair<SingleAnimInfo, int>[] animations;

    /// <summary>
    /// 空动画信息实例
    /// 用于表示无效或空的动画集合
    /// </summary>
    public static AnimInfo Null = new AnimInfo("", null, 0);

    /// <summary>
    /// 创建动画信息
    /// </summary>
    /// <param name="key">动画集合标识符</param>
    /// <param name="animations">动画数组（动画信息和权重的键值对）</param>
    /// <param name="priority">优先级</param>
    public AnimInfo(string key, KeyValuePair<SingleAnimInfo, int>[] animations, int priority = 0)
    {
        this.animations = animations;
        this.priority = priority;
        this.key = key;
    }

    /// <summary>
    /// 根据权重随机选择一个动画
    /// </summary>
    /// <returns>随机选择的动画信息</returns>
    public SingleAnimInfo RandomKey()
    {
        // 检查动画数组是否为空
        if (animations == null || animations.Length <= 0) 
            return SingleAnimInfo.Null;

        // 如果只有一个动画，直接返回
        if (animations.Length == 1) 
            return animations[0].Key;

        // 计算所有动画权重总和
        int totalWeight = 0;
        for (int i = 0; i < animations.Length; i++)
        {
            totalWeight += animations[i].Value;
        }
        
        // 如果总权重为0或负数，返回空动画
        if (totalWeight <= 0) 
            return SingleAnimInfo.Null;

        // 按权重随机选择一个动画
        int randomValue = Random.Range(0, totalWeight);
        int currentIndex = 0;
        
        while (randomValue > 0)
        {
            randomValue -= animations[currentIndex].Value;
            currentIndex += 1;
        }
        
        // 确保索引不超出数组范围
        currentIndex = Mathf.Min(currentIndex, animations.Length - 1);
        return animations[currentIndex].Key;
    }

    /// <summary>
    /// 检查动画集合中是否包含指定名称的动画
    /// </summary>
    /// <param name="animName">要查找的动画名称</param>
    /// <returns>如果包含指定动画则返回true，否则返回false</returns>
    public bool ContainsAnim(string animName)
    {
        if (animations == null)
            return false;
            
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i].Key.animName == animName) 
                return true;
        }
        return false;
    }
}

/// <summary>
/// 单个动画信息结构体
/// 存储单个动画的名称和持续时间
/// </summary>
public struct SingleAnimInfo
{
    /// <summary>
    /// 动画名称
    /// </summary>
    public string animName;

    /// <summary>
    /// 动画持续时间（秒）
    /// </summary>
    public float duration;

    /// <summary>
    /// 创建单个动画信息
    /// </summary>
    /// <param name="animName">动画名称</param>
    /// <param name="duration">动画持续时间</param>
    public SingleAnimInfo(string animName, float duration = 0)
    {
        this.animName = animName;
        this.duration = duration;
    }

    /// <summary>
    /// 空动画信息实例
    /// 用于表示无效或空的动画
    /// </summary>
    public static SingleAnimInfo Null = new SingleAnimInfo("", 0);
}
