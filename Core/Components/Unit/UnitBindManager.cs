using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位绑定点管理器：管理单个角色/单位上的所有绑定点
/// 提供统一的接口来添加、移除和查找绑定点上的游戏对象
/// </summary>
public class UnitBindManager : MonoBehaviour
{
    /// <summary>
    /// 根据键名获取绑定点组件
    /// </summary>
    /// <param name="key">绑定点的键名标识符</param>
    /// <returns>找到的绑定点组件，未找到时返回null</returns>
    public UnitBindPoint GetBindPointByKey(string key)
    {
        UnitBindPoint[] bindPoints = this.gameObject.GetComponentsInChildren<UnitBindPoint>();
        for (int i = 0; i < bindPoints.Length; i++)
        {
            if (bindPoints[i].key == key)
            {
                return bindPoints[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 在指定的绑定点上添加游戏对象
    /// </summary>
    /// <param name="bindPointKey">目标绑定点的键名</param>
    /// <param name="go">要添加的游戏对象资源路径</param>
    /// <param name="key">绑定对象的唯一标识符</param>
    /// <param name="loop">是否循环播放（永久绑定）</param>
    public void AddBindGameObject(string bindPointKey, string go, string key, bool loop)
    {
        UnitBindPoint bp = GetBindPointByKey(bindPointKey);
        if (bp == null)
            return;
        bp.AddBindGameObject(go, key, loop);
    }

    /// <summary>
    /// 从指定的绑定点移除游戏对象
    /// </summary>
    /// <param name="bindPointKey">目标绑定点的键名</param>
    /// <param name="key">要移除的绑定对象标识符</param>
    public void RemoveBindGameObject(string bindPointKey, string key)
    {
        UnitBindPoint bp = GetBindPointByKey(bindPointKey);
        if (bp == null)
            return;
        bp.RemoveBindGameObject(key);
    }

    /// <summary>
    /// 从所有绑定点移除指定标识符的游戏对象
    /// </summary>
    /// <param name="key">要移除的绑定对象标识符</param>
    public void RemoveAllBindGameObject(string key)
    {
        UnitBindPoint[] bindPoints = this.gameObject.GetComponentsInChildren<UnitBindPoint>();
        for (int i = 0; i < bindPoints.Length; i++)
        {
            bindPoints[i].RemoveBindGameObject(key);
        }
    }
}
