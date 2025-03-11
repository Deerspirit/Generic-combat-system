using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位绑定点组件：用于在角色/单位的特定位置绑定视觉效果或其他游戏对象
/// 支持永久绑定或限时绑定效果，通常用于技能、buff效果的显示
/// </summary>
public class UnitBindPoint : MonoBehaviour
{
    /// <summary>
    /// 绑定点标识符，用于区分不同绑定点（如"head"、"hand_r"等）
    /// </summary>
    public string key;

    /// <summary>
    /// 绑定点位置偏移量，相对于当前变换的本地坐标
    /// </summary>
    public Vector3 offset;

    /// <summary>
    /// 当前绑定的所有游戏对象信息，键为绑定对象的唯一标识符
    /// </summary>
    private Dictionary<string, BindGameObjectInfo> bindGameObject = new Dictionary<string, BindGameObjectInfo>();

    /// <summary>
    /// 更新绑定对象的生命周期，移除过期的绑定对象
    /// </summary>
    private void FixedUpdate()
    {
        List<string> toRemove = new List<string>();
        
        // 检查所有绑定对象
        foreach (KeyValuePair<string, BindGameObjectInfo> goInfo in bindGameObject)
        {
            // 如果对象已被销毁，标记为移除
            if (goInfo.Value.gameObject == null)
            {
                toRemove.Add(goInfo.Key);
                continue;
            }
            
            // 如果不是永久绑定，更新持续时间
            if (goInfo.Value.forever == false)
            {
                goInfo.Value.duration -= Time.fixedDeltaTime;
                // 当持续时间结束，销毁对象并标记为移除
                if (goInfo.Value.duration <= 0)
                {
                    Destroy(goInfo.Value.gameObject);
                    toRemove.Add(goInfo.Key);
                }
            }
        }
        
        // 从绑定字典中移除标记的对象
        for (int i = 0; i < toRemove.Count; i++)
        {
            bindGameObject.Remove(toRemove[i]);
        }
    }

    /// <summary>
    /// 添加绑定游戏对象到当前绑定点
    /// </summary>
    /// <param name="goPath">游戏对象资源路径</param>
    /// <param name="key">绑定对象的唯一标识符（为空时自动生成）</param>
    /// <param name="loop">是否循环播放（永久绑定）</param>
    public void AddBindGameObject(string goPath, string key, bool loop)
    {
        // 如果指定了key且已存在相同key的绑定，则不执行
        if (key != "" && bindGameObject.ContainsKey(key) == true)
            return;

        // 实例化游戏对象
        GameObject effectGo = Instantiate<GameObject>(
            Resources.Load<GameObject>(goPath),
            Vector3.zero,
            Quaternion.identity,
            this.gameObject.transform
        );
        
        // 设置本地位置和旋转
        effectGo.transform.localPosition = this.offset;
        effectGo.transform.localRotation = Quaternion.identity;
        
        if (!effectGo)
            return;
            
        // 检查是否包含SightEffect组件
        SightEffect se = effectGo.GetComponent<SightEffect>();
        if (!se)
        {
            Destroy(effectGo);
            return;
        }
        
        // 设置持续时间，负值表示永久绑定
        float duration = se.duration * (loop == false ? 1 : -1);
        BindGameObjectInfo bindGameObjectInfo = new BindGameObjectInfo(effectGo, duration);
        
        // 添加到绑定字典
        if (key != "")
        {
            this.bindGameObject.Add(key, bindGameObjectInfo);
        }
        else
        {
            // 生成随机key
            this.bindGameObject.Add(
                Time.frameCount * Random.Range(1.00f, 9.99f) + "_" + Random.Range(1, 9999),
                bindGameObjectInfo
            );
        }
    }
    
    /// <summary>
    /// 移除指定key的绑定游戏对象
    /// </summary>
    /// <param name="key">要移除的绑定对象标识符</param>
    public void RemoveBindGameObject(string key)
    {
        if (bindGameObject.ContainsKey(key) == false)
            return;
            
        // 销毁游戏对象
        if (bindGameObject[key].gameObject)
        {
            Destroy(bindGameObject[key].gameObject);
        }
        
        // 从字典中移除
        bindGameObject.Remove(key);
    }
}

/// <summary>
/// 绑定游戏对象信息类：存储绑定对象的引用和生命周期信息
/// </summary>
public class BindGameObjectInfo
{
    /// <summary>
    /// 绑定的游戏对象引用
    /// </summary>
    public GameObject gameObject;
    
    /// <summary>
    /// 剩余持续时间（秒）
    /// </summary>
    public float duration;
    
    /// <summary>
    /// 是否永久绑定
    /// </summary>
    public bool forever;
    
    /// <summary>
    /// 创建绑定游戏对象信息
    /// </summary>
    /// <param name="gameObject">要绑定的游戏对象</param>
    /// <param name="duration">持续时间（负值表示永久绑定）</param>
    public BindGameObjectInfo(GameObject gameObject, float duration)
    {
        this.gameObject = gameObject;
        this.duration = Mathf.Abs(duration);
        this.forever = duration <= 0;
    }
}
