using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相机跟随系统：使相机平滑跟随目标角色
/// 维护相机与角色之间的相对位置关系，确保游戏视角的稳定性
/// </summary>
public class CamFollow : MonoBehaviour
{
    #region 私有属性
    /// <summary>
    /// 相机与目标角色的相对偏移量
    /// </summary>
    private Vector3 cameraOffset;

    /// <summary>
    /// 当前跟随的目标角色
    /// </summary>
    private GameObject targetCharacter;

    /// <summary>
    /// 相机跟随平滑度（值越小，相机移动越平滑）
    /// </summary>
    [SerializeField]
    private float smoothSpeed = 0.125f;

    /// <summary>
    /// 是否使用平滑跟随，false则立即跟随
    /// </summary>
    [SerializeField]
    private bool useSmoothFollow = true;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 在所有Update函数调用后执行，用于相机跟随
    /// 相机移动应在LateUpdate中进行，以确保所有角色已经完成移动
    /// </summary>
    void LateUpdate()
    {
        // 检查目标角色是否有效
        if (!targetCharacter) 
            return;
        
        // 计算目标位置
        Vector3 targetPosition = targetCharacter.transform.position + cameraOffset;
        
        if (useSmoothFollow)
        {
            // 平滑插值移动相机
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        }
        else
        {
            // 直接移动相机
            transform.position = targetPosition;
        }
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 设置相机跟随的目标角色
    /// </summary>
    /// <param name="character">要跟随的角色</param>
    public void SetFollowCharacter(GameObject character)
    {
        if (character == null)
            return;
            
        targetCharacter = character;
        
        // 计算相机与角色的偏移量，保持相机的当前视角
        cameraOffset = new Vector3(
            transform.position.x - character.transform.position.x,
            transform.position.y - character.transform.position.y,
            transform.position.z - character.transform.position.z
        );
    }

    /// <summary>
    /// 设置相机跟随的平滑度
    /// </summary>
    /// <param name="speed">平滑度值（0-1之间）</param>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Clamp(speed, 0.01f, 1f);
    }

    /// <summary>
    /// 设置是否使用平滑跟随
    /// </summary>
    /// <param name="useSmooth">是否平滑</param>
    public void SetUseSmoothFollow(bool useSmooth)
    {
        useSmoothFollow = useSmooth;
    }
    #endregion
}
