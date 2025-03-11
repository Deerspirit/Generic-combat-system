using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色控制状态结构体：定义角色的基本行为控制权限
/// 用于控制角色是否能够移动、旋转和使用技能
/// 可通过各种效果（如眩晕、沉默等）修改角色的控制状态
/// </summary>
public struct ChaControlState
{
    /// <summary>
    /// 角色是否能够移动
    /// </summary>
    public bool canMove;
    
    /// <summary>
    /// 角色是否能够旋转
    /// </summary>
    public bool canRotate;
    
    /// <summary>
    /// 角色是否能够使用技能
    /// </summary>
    public bool canUseSkill;

    /// <summary>
    /// 创建角色控制状态实例
    /// </summary>
    /// <param name="canMove">是否可以移动，默认为true</param>
    /// <param name="canRotate">是否可以旋转，默认为true</param>
    /// <param name="canUseSkill">是否可以使用技能，默认为true</param>
    public ChaControlState(bool canMove = true, bool canRotate = true, bool canUseSkill = true)
    {
        this.canMove = canMove;
        this.canRotate = canRotate;
        this.canUseSkill = canUseSkill;
    }

    /// <summary>
    /// 重置为原始状态（所有权限都为true）
    /// </summary>
    public void Origin()
    {
        this.canMove = true;
        this.canRotate = true;
        this.canUseSkill = true;
    }

    /// <summary>
    /// 原始状态常量：所有权限都为true
    /// </summary>
    public static ChaControlState origin = new ChaControlState(true, true, true);

    /// <summary>
    /// 眩晕状态常量：所有权限都为false
    /// </summary>
    public static ChaControlState stun = new ChaControlState(false, false, false);

    /// <summary>
    /// 运算符重载：组合两个控制状态
    /// 当某个权限在两个状态中都为true时，结果才为true
    /// </summary>
    /// <param name="cs1">第一个控制状态</param>
    /// <param name="cs2">第二个控制状态</param>
    /// <returns>组合后的控制状态</returns>
    public static ChaControlState operator +(ChaControlState cs1, ChaControlState cs2)
    {
        return new ChaControlState(
            cs1.canMove && cs2.canMove,
            cs1.canRotate && cs2.canRotate,
            cs1.canUseSkill && cs2.canUseSkill
        );
    }
}
