using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制器：处理玩家输入并转换为角色行为
/// 负责接收键盘、鼠标输入并对角色发出移动、旋转和技能指令
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 主摄像机引用，用于计算鼠标位置和角色旋转
    /// </summary>
    public Camera mainCamera;

    /// <summary>
    /// 角色状态组件引用
    /// </summary>
    private ChaState chaState;

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    void Start()
    {
        chaState = this.gameObject.GetComponent<ChaState>();
    }

    /// <summary>
    /// 处理玩家输入并转换为角色行为
    /// </summary>
    void FixedUpdate()
    {
        // 检查角色状态是否有效
        if (!chaState || chaState.dead == true) return;

        // 获取水平和垂直输入轴
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // 获取技能按键状态
        bool[] skillButtons = new bool[]{
            Input.GetButton("Fire5"),  // F键
            Input.GetButton("Fire4"),  // E键
            Input.GetButton("Fire3"),  // R键
            Input.GetButton("Fire2"),  // 鼠标右键
            Input.GetButton("Fire1"),  // 鼠标左键
            Input.GetButton("Jump")    // 空格键
        };

        // 获取鼠标在屏幕上的位置
        Vector2 cursorPosition = Input.mousePosition;

        // 处理角色旋转（面向鼠标方向）
        float rotationAngle = transform.rotation.eulerAngles.y;
        if (mainCamera)
        {
            // 将角色位置转换为屏幕坐标
            Vector2 characterScreenPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);
            
            // 计算角色到鼠标的方向角度（度）
            rotationAngle = Mathf.Atan2(
                cursorPosition.x - characterScreenPosition.x, 
                cursorPosition.y - characterScreenPosition.y
            ) * 180.00f / Mathf.PI;
            
            // 发送旋转指令
            chaState.OrderRotateTo(rotationAngle);
        }

        // 处理角色移动
        if (horizontalInput != 0 || verticalInput != 0)
        {
            float moveSpeed = chaState.moveSpeed;
            Vector3 moveVector = new Vector3(horizontalInput * moveSpeed, 0, verticalInput * moveSpeed);
            chaState.OrderMove(moveVector);
        }

        // 技能ID数组，与按键对应
        string[] skillIds = new string[]{
             "explosiveBarrel", "teleportBullet","grenade","cloakBoomerang","fire","roll"
        };

        // 处理技能释放
        bool isAnySkillButtonPressed = false;
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] == true)
            {
                // 尝试释放对应技能
                chaState.CastSkill(skillIds[i]);
                isAnySkillButtonPressed = true;
            }
        }
        
        // 更新角色蓄力状态
        chaState.charging = isAnySkillButtonPressed;
    }
}
