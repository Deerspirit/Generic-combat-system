using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视觉特效基类：所有视觉特效的基础组件
/// 提供特效的基本属性和生命周期管理
/// 可被UnitBindPoint和其他系统引用来控制特效的持续时间
/// </summary>
public class SightEffect : MonoBehaviour
{
    /// <summary>
    /// 特效的总持续时间（秒）
    /// 当特效被UnitBindPoint引用时，此值决定特效的默认生命周期
    /// </summary>
    [Tooltip("特效总时长，单位：秒")]
    public float duration = 1.0f;
    
    // 可在此添加特效的通用方法，如：
    // - 播放/停止特效
    // - 调整特效参数（大小、颜色、强度等）
    // - 特效事件回调
}
