using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家状态监听器：用于将玩家角色状态数据实时更新到UI界面
/// 主要监听玩家生命值变化并在UI中显示
/// </summary>
public class PlayerStateListener : MonoBehaviour
{
    /// <summary>
    /// 玩家角色游戏对象引用
    /// </summary>
    [Tooltip("玩家的角色（核心的那个）的GameObject")]
    public GameObject playerGameObject;

    /// <summary>
    /// UI文本组件，用于显示玩家状态
    /// </summary>
    private Text textComponent;
    
    /// <summary>
    /// 玩家角色状态组件引用
    /// </summary>
    private ChaState playerState;

    /// <summary>
    /// 初始化文本组件
    /// </summary>
    private void Start()
    {
        textComponent = this.gameObject.GetComponent<Text>();
    }

    /// <summary>
    /// 每帧更新玩家状态到UI
    /// </summary>
    private void Update()
    {
        // 检查必要组件是否存在
        if (playerGameObject == null || textComponent == null) return;
        
        // 获取玩家状态组件（如果尚未获取）
        if (playerState == null) 
            playerState = playerGameObject.GetComponent<ChaState>();
        
        if (playerState == null) return;
        
        // 获取当前生命值和最大生命值
        int currentHp = playerState.resource.hp;
        int maxHp = playerState.property.hp;
        
        // 根据生命值百分比决定文本颜色（绿色=安全，红色=危险）
        string healthColor = (currentHp * 1.000f / (maxHp * 1.000f)) > 0.300f ? "green" : "red";
        
        // 更新UI文本，显示生命值和颜色
        textComponent.text = $"<color={healthColor}>{currentHp} / {maxHp}</color>";
    }
}