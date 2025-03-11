using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹出文本管理器：负责在游戏世界中的角色头顶创建弹出文本效果
/// 用于显示伤害数值、治疗数值、状态文本等信息
/// </summary>
public class PopTextManager : MonoBehaviour
{
    /// <summary>
    /// 在角色头顶弹出数字文本
    /// </summary>
    /// <param name="cha">目标角色游戏对象</param>
    /// <param name="value">要显示的数值</param>
    /// <param name="asHeal">是否为治疗效果（绿色）</param>
    /// <param name="asCritical">是否为暴击效果（更大字体）</param>
    public void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false)
    {
        // 检查目标角色是否有效
        if (!cha) return;
        
        // 将世界坐标转换为屏幕坐标
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, cha.transform.position);

        // 实例化弹出文本预制体
        GameObject textObject = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/UI/PopText"),
            screenPosition,
            Quaternion.identity,
            this.gameObject.transform
        );

        // 设置弹出文本的目标角色，用于跟随
        textObject.GetComponent<UnitPopText>().target = cha;

        // 设置文本内容、颜色和大小
        Text textComponent = textObject.GetComponent<Text>();
        string colorTag = asHeal ? "green" : "red";
        string prefix = asHeal ? "+" : "-";
        string suffix = asCritical ? "!" : "";
        
        textComponent.text = $"<color={colorTag}>{prefix}{value}{suffix}</color>";
        textComponent.fontSize = asCritical ? 40 : 30;
    }

    /// <summary>
    /// 在角色头顶弹出字符串文本
    /// </summary>
    /// <param name="cha">目标角色游戏对象</param>
    /// <param name="text">要显示的文本内容</param>
    /// <param name="size">文本字体大小</param>
    public void PopUpStringOnCharacter(GameObject cha, string text, int size = 30)
    {
        // 检查目标角色是否有效
        if (!cha) return;
        
        // 将世界坐标转换为屏幕坐标
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, cha.transform.position);

        // 实例化弹出文本预制体
        GameObject textObject = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/UI/PopText"),
            screenPosition,
            Quaternion.identity,
            this.gameObject.transform
        );

        // 设置弹出文本的目标角色，用于跟随
        textObject.GetComponent<UnitPopText>().target = cha;

        // 设置文本内容和大小
        Text textComponent = textObject.GetComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = size;
    }
}