using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位移除器：自动在指定时间后销毁游戏对象
/// 通常用于临时特效、弹药壳、粒子系统等需要自动清理的游戏元素
/// </summary>
public class UnitRemover : MonoBehaviour
{
    /// <summary>
    /// 游戏对象存在的持续时间（秒）
    /// </summary>
    [Tooltip("多久以后把我的gameObject干掉，单位：秒")]
    public float duration = 1.0f;

    /// <summary>
    /// 每帧更新持续时间并在时间结束时销毁游戏对象
    /// </summary>
    private void FixedUpdate()
    {
        duration -= Time.fixedDeltaTime;
        if (duration <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}