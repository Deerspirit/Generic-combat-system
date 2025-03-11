using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹状态类：管理子弹的运动、碰撞和生命周期
/// 作为战斗系统的核心组件之一，处理子弹的移动轨迹、碰撞检测和效果触发
/// </summary>
public class BulletState : MonoBehaviour
{
    #region 子弹属性
    /// <summary>
    /// 子弹模型数据
    /// </summary>
    public BulletModel model;

    /// <summary>
    /// 子弹发射者
    /// </summary>
    public GameObject caster;

    /// <summary>
    /// 发射时施放者的属性（用于伤害计算）
    /// </summary>
    public ChaProperty propWhileCast = ChaProperty.zero;

    /// <summary>
    /// 发射角度
    /// </summary>
    public float fireDegree;

    /// <summary>
    /// 子弹速度
    /// </summary>
    public float speed;

    /// <summary>
    /// 子弹持续时间
    /// </summary>
    public float duration;

    /// <summary>
    /// 子弹已存在的时间
    /// </summary>
    public float timeElapsed = 0;

    /// <summary>
    /// 子弹轨迹函数
    /// </summary>
    public BulletTween tween = null;

    /// <summary>
    /// 子弹移动力
    /// </summary>
    private Vector3 moveForce = new Vector3();

    /// <summary>
    /// 子弹当前速度
    /// </summary>
    public Vector3 velocity
    {
        get { return moveForce; }
    }

    /// <summary>
    /// 是否始终使用发射角度（不根据实际运动方向调整角度）
    /// </summary>
    public bool useFireDegreeForever = false;

    /// <summary>
    /// 子弹命中记录（防止重复命中同一目标）
    /// </summary>
    public List<BulletHitRecord> hitRecords = new List<BulletHitRecord>();

    /// <summary>
    /// 创建后无法命中的时间（秒）
    /// </summary>
    public float canHitAfterCreated = 0;

    /// <summary>
    /// 追踪的目标
    /// </summary>
    public GameObject followingTarget = null;

    /// <summary>
    /// 自定义参数
    /// </summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    /// <summary>
    /// 子弹生命值（可以命中的次数）
    /// </summary>
    public int hp = 1;
    #endregion

    #region 移动相关
    /// <summary>
    /// 移动类型
    /// </summary>
    private MoveType moveType;

    /// <summary>
    /// 是否使用平滑移动
    /// </summary>
    private bool smoothMove;
    #endregion

    #region 组件引用
    private UnitRotate unitRotate;
    private UnitMove unitMove;
    private GameObject viewContainer;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 初始化子弹
    /// </summary>
    private void Start()
    {
        synchronizedUnits();
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 检查子弹是否碰到障碍物
    /// </summary>
    /// <returns>是否碰到障碍物</returns>
    public bool HitObstacle()
    {
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    /// <summary>
    /// 设置子弹的移动力和方向
    /// </summary>
    /// <param name="mf">移动力向量</param>
    public void SetMoveForce(Vector3 mf)
    {
        this.moveForce = mf;

        // 确定移动角度
        float moveDegree = (useFireDegreeForever || timeElapsed <= 0) 
            ? fireDegree 
            : transform.rotation.eulerAngles.y;

        // 计算移动力
        moveForce.y = 0; // 保持在水平面上移动
        moveForce *= speed;
        moveDegree += Mathf.Atan2(moveForce.x, moveForce.z) * Mathf.Rad2Deg;

        // 根据移动角度调整移动力的方向
        float moveRadians = moveDegree * Mathf.Deg2Rad;
        float moveLength = Mathf.Sqrt(Mathf.Pow(moveForce.x, 2) + Mathf.Pow(moveForce.z, 2));

        moveForce.x = Mathf.Sin(moveRadians) * moveLength;
        moveForce.z = Mathf.Cos(moveRadians) * moveLength;

        // 应用移动和旋转
        unitMove.MoveBy(moveForce);
        unitRotate.RotateTo(moveDegree);
    }

    /// <summary>
    /// 通过子弹发射器初始化子弹状态
    /// </summary>
    /// <param name="bullet">子弹发射器</param>
    /// <param name="targets">可能的目标列表</param>
    public void InitByBulletLauncher(BulletLauncher bullet, GameObject[] targets)
    {
        // 设置基本属性
        this.model = bullet.model;
        this.caster = bullet.caster;
        
        // 保存施放者属性（用于伤害计算）
        if (this.caster && caster.GetComponent<ChaState>())
        {
            this.propWhileCast = caster.GetComponent<ChaState>().property;
        }
        
        // 设置运动参数
        this.fireDegree = bullet.fireDegree;
        this.speed = bullet.speed;
        this.duration = bullet.duration;
        this.timeElapsed = 0;
        this.tween = bullet.tween;
        this.useFireDegreeForever = bullet.useFireDegreeForever;
        this.canHitAfterCreated = bullet.canHitAfterCreated;
        this.smoothMove = !bullet.model.removeOnObstacle;
        this.moveType = bullet.model.moveType;
        this.hp = bullet.model.hitTimes;

        // 设置自定义参数
        this.param = new Dictionary<string, object>();
        if (bullet.param != null)
        {
            foreach (var parameter in bullet.param)
            {
                this.param.Add(parameter.Key, parameter.Value);
            }
        }

        // 确保组件已同步
        synchronizedUnits();

        // 创建视觉效果
        if (!string.IsNullOrEmpty(bullet.model.prefab))
        {
            CreateVisualEffect(bullet.model.prefab);
        }

        // 重置位置的Y坐标
        this.gameObject.transform.position = new Vector3(
            this.gameObject.transform.position.x,
            0,
            this.gameObject.transform.position.z
        );

        // 设置追踪目标
        this.followingTarget = bullet.targetFunc == null 
            ? null 
            : bullet.targetFunc(this.gameObject, targets);
    }

    /// <summary>
    /// 设置子弹的移动类型
    /// </summary>
    /// <param name="toType">目标移动类型</param>
    public void SetMoveType(MoveType toType)
    {
        this.moveType = toType;
        synchronizedUnits();
    }

    /// <summary>
    /// 检查是否可以命中指定目标
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <returns>是否可以命中</returns>
    public bool CanHit(GameObject target)
    {
        // 创建后无法命中的时间尚未过去
        if (canHitAfterCreated > 0) 
            return false;
            
        // 检查是否已命中过该目标
        foreach (var record in hitRecords)
        {
            if (record.target == target)
            {
                return false;
            }
        }

        // 检查目标是否处于免疫状态
        ChaState targetState = target.GetComponent<ChaState>();
        if (targetState && targetState.immuneTime > 0) 
            return false;

        return true;
    }

    /// <summary>
    /// 添加命中记录
    /// </summary>
    /// <param name="target">命中的目标</param>
    public void AddHitRecord(GameObject target)
    {
        hitRecords.Add(new BulletHitRecord(
            target,
            this.model.sameTargetDelay
        ));
    }

    /// <summary>
    /// 设置子弹碰撞半径
    /// </summary>
    /// <param name="radius">碰撞半径</param>
    public void SetRadius(float radius)
    {
        this.model.radius = radius;
        synchronizedUnits();
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 同步获取组件引用
    /// </summary>
    private void synchronizedUnits()
    {
        if (!unitRotate) unitRotate = GetComponent<UnitRotate>();
        if (!unitMove) unitMove = GetComponent<UnitMove>();
        if (!viewContainer) viewContainer = GetComponentInChildren<ViewContainer>()?.gameObject;

        if (unitMove)
        {
            unitMove.smoothMove = this.smoothMove;
            unitMove.moveType = this.moveType;
            unitMove.bodyRadius = this.model.radius;
        }
    }

    /// <summary>
    /// 创建子弹的视觉效果
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    private void CreateVisualEffect(string prefabPath)
    {
        GameObject bulletEffect = Instantiate(
            Resources.Load<GameObject>("Prefabs/Bullet/" + prefabPath),
            Vector3.zero,
            Quaternion.identity,
            viewContainer.transform
        );
        
        bulletEffect.transform.localPosition = new Vector3(0, transform.position.y, 0);
        bulletEffect.transform.localRotation = Quaternion.identity;
    }
    #endregion
}