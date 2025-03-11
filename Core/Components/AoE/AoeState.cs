using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 范围效果状态：管理AOE的生命周期、碰撞检测和效果处理
/// 作为战斗系统的核心组件之一，处理范围伤害、区域效果和持续性影响
/// </summary>
public class AoeState : MonoBehaviour
{
    #region AOE属性
    /// <summary>
    /// AOE模型数据
    /// </summary>
    public AoeModel model;

    /// <summary>
    /// 是否刚创建（用于初始碰撞检测）
    /// </summary>
    public bool justCreated = true;

    /// <summary>
    /// 效果半径
    /// </summary>
    public float radius;

    /// <summary>
    /// 效果施放者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// 效果持续时间
    /// </summary>
    public float duration;

    /// <summary>
    /// 已经存在的时间
    /// </summary>
    public float timeElapsed = 0;

    /// <summary>
    /// 移动轨迹函数
    /// </summary>
    public AoeTween tween;

    /// <summary>
    /// 轨迹函数已运行时间
    /// </summary>
    public float tweenRunnedTime = 0;

    /// <summary>
    /// 创建时施放者的属性（用于效果计算）
    /// </summary>
    public ChaProperty propWhileCreate;

    /// <summary>
    /// 自定义参数
    /// </summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    /// <summary>
    /// 范围内的角色列表
    /// </summary>
    public List<GameObject> characterInRange = new List<GameObject>();

    /// <summary>
    /// 范围内的子弹列表
    /// </summary>
    public List<GameObject> bulletInRange = new List<GameObject>();

    /// <summary>
    /// 轨迹函数参数
    /// </summary>
    public object[] tweenParam;

    /// <summary>
    /// 当前移动速度
    /// </summary>
    public Vector3 velocity
    {
        get { return this._velocity; }
    }
    private Vector3 _velocity = new Vector3();
    #endregion

    #region 组件引用
    private UnitMove unitMove;
    private UnitRotate unitRotate;
    private GameObject viewContainer;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 初始化AOE
    /// </summary>
    private void Start()
    {
        synchronizeComponents();
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 设置AOE的移动和旋转
    /// </summary>
    /// <param name="aoeMoveInfo">移动信息</param>
    public void SetMoveAndRotate(AoeMoveInfo aoeMoveInfo)
    {
        if (aoeMoveInfo == null) 
            return;
            
        // 设置移动
        if (unitMove)
        {
            unitMove.moveType = aoeMoveInfo.moveType;
            unitMove.bodyRadius = this.radius;
            _velocity = aoeMoveInfo.velocity / Time.fixedDeltaTime;
            unitMove.MoveBy(_velocity);
        }
        
        // 设置旋转
        if (unitRotate)
        {
            unitRotate.RotateTo(aoeMoveInfo.rotateToDegree);
        }
    }

    /// <summary>
    /// 检查是否碰到障碍物
    /// </summary>
    /// <returns>是否碰到障碍物</returns>
    public bool HitObstacle()
    {
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    /// <summary>
    /// 通过AOE发射器初始化AOE状态
    /// </summary>
    /// <param name="aoe">AOE发射器</param>
    public void InitByAoeLauncher(AoeLauncher aoe)
    {
        // 设置基本属性
        this.model = aoe.model;
        this.radius = aoe.radius;
        this.duration = aoe.duration;
        this.timeElapsed = 0;
        this.tween = aoe.tween;
        this.tweenParam = aoe.tweenParam;
        this.tweenRunnedTime = 0;
        
        // 设置自定义参数
        this.param = new Dictionary<string, object>();
        foreach (var parameter in aoe.param)
        {
            this.param[parameter.Key] = parameter.Value;
        }
        
        // 设置施放者和属性
        this.caster = aoe.caster;
        this.propWhileCreate = aoe.caster 
            ? aoe.caster.GetComponent<ChaState>()?.property ?? ChaProperty.zero 
            : ChaProperty.zero;

        // 设置位置和角度
        transform.position = aoe.position;
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = aoe.degree;
        transform.eulerAngles = eulerAngles;

        // 确保组件已同步
        synchronizeComponents();

        // 创建视觉效果
        if (!string.IsNullOrEmpty(aoe.model.prefab))
        {
            CreateVisualEffect(aoe.model.prefab);
        }
        
        // 重置Y坐标
        transform.position = new Vector3(
            transform.position.x,
            0,
            transform.position.z
        );
    }

    /// <summary>
    /// 设置视觉效果的缩放
    /// </summary>
    /// <param name="scaleX">X轴缩放</param>
    /// <param name="scaleY">Y轴缩放</param>
    /// <param name="scaleZ">Z轴缩放</param>
    public void SetViewScale(float scaleX = 1, float scaleY = 1, float scaleZ = 1)
    {
        synchronizeComponents();
        
        if (viewContainer != null)
        {
            Vector3 scale = viewContainer.transform.localScale;
            scale.Set(scaleX, scaleY, scaleZ);
            viewContainer.transform.localScale = scale;
        }
    }

    /// <summary>
    /// 修改视觉效果的Y坐标
    /// </summary>
    /// <param name="toY">目标Y坐标</param>
    public void ModViewY(float toY)
    {
        if (viewContainer != null)
        {
            viewContainer.transform.position = new Vector3(
                viewContainer.transform.position.x,
                toY,
                viewContainer.transform.position.z
            );
        }
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 同步获取组件引用
    /// </summary>
    private void synchronizeComponents()
    {
        if (!unitMove) unitMove = GetComponent<UnitMove>();
        if (!unitRotate) unitRotate = GetComponent<UnitRotate>();
        if (!viewContainer) viewContainer = GetComponentInChildren<ViewContainer>()?.gameObject;
        
        if (unitMove != null)
        {
            unitMove.bodyRadius = this.radius;
            unitMove.smoothMove = !model.removeOnObstacle;
        }
    }

    /// <summary>
    /// 创建AOE的视觉效果
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    private void CreateVisualEffect(string prefabPath)
    {
        GameObject aoeEffect = Instantiate(
            Resources.Load<GameObject>("Prefabs/" + prefabPath),
            Vector3.zero,
            Quaternion.identity,
            viewContainer.transform
        );

        aoeEffect.transform.localPosition = new Vector3(0, transform.position.y, 0);
        aoeEffect.transform.localRotation = Quaternion.identity;
    }
    #endregion
}