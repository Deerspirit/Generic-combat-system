using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AOE(范围效果)管理器：处理游戏中所有范围效果的创建、碰撞检测和生命周期
/// </summary>
public class AoeManager : MonoBehaviour
{
    #region Unity生命周期
    private void FixedUpdate()
    {
        ProcessAllAoeEffects();
    }
    #endregion

    #region AOE处理
    /// <summary>
    /// 处理所有AOE效果
    /// </summary>
    private void ProcessAllAoeEffects()
    {
        GameObject[] aoeObjects = GameObject.FindGameObjectsWithTag("AoE");
        if (aoeObjects.Length <= 0) return;

        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        float deltaTime = Time.fixedDeltaTime;

        foreach (var aoeObject in aoeObjects)
        {
            ProcessSingleAoeEffect(aoeObject, characters, bullets, deltaTime);
        }
    }

    /// <summary>
    /// 处理单个AOE效果
    /// </summary>
    private void ProcessSingleAoeEffect(GameObject aoeObject, GameObject[] characters, GameObject[] bullets, float deltaTime)
    {
        AoeState aoeState = aoeObject.GetComponent<AoeState>();
        if (!aoeState) return;

        // 处理AOE移动
        ProcessAoeMovement(aoeObject, aoeState, deltaTime);

        // 处理AOE效果
        if (aoeState.justCreated)
        {
            HandleNewAoeEffect(aoeObject, aoeState, characters, bullets);
        }
        else
        {
            HandleExistingAoeEffect(aoeObject, aoeState, characters, bullets);
        }

        // 更新AOE生命周期
        UpdateAoeLifecycle(aoeObject, aoeState, deltaTime);
    }

    /// <summary>
    /// 处理AOE移动
    /// </summary>
    private void ProcessAoeMovement(GameObject aoeObject, AoeState aoeState, float deltaTime)
    {
        if (aoeState.duration > 0 && aoeState.tween != null)
        {
            AoeMoveInfo moveInfo = aoeState.tween(aoeObject, aoeState.tweenRunnedTime);
            aoeState.tweenRunnedTime += deltaTime;
            aoeState.SetMoveAndRotate(moveInfo);
        }
    }

    /// <summary>
    /// 处理新创建的AOE效果
    /// </summary>
    private void HandleNewAoeEffect(GameObject aoeObject, AoeState aoeState, GameObject[] characters, GameObject[] bullets)
    {
        aoeState.justCreated = false;

        // 检测范围内的角色
        foreach (var character in characters)
        {
            if (IsObjectInRange(aoeObject, character, aoeState.radius))
            {
                aoeState.characterInRange.Add(character);
            }
        }

        // 检测范围内的子弹
        foreach (var bullet in bullets)
        {
            if (IsObjectInRange(aoeObject, bullet, aoeState.radius))
            {
                aoeState.bulletInRange.Add(bullet);
            }
        }

        // 触发创建事件
        aoeState.model.onCreate?.Invoke(aoeObject);
    }

    /// <summary>
    /// 处理已存在的AOE效果
    /// </summary>
    private void HandleExistingAoeEffect(GameObject aoeObject, AoeState aoeState, GameObject[] characters, GameObject[] bullets)
    {
        // 处理角色离开和进入
        ProcessCharacterMovement(aoeObject, aoeState, characters);

        // 处理子弹离开和进入
        ProcessBulletMovement(aoeObject, aoeState, bullets);
    }

    /// <summary>
    /// 处理角色的进入和离开
    /// </summary>
    private void ProcessCharacterMovement(GameObject aoeObject, AoeState aoeState, GameObject[] characters)
    {
        // 处理离开的角色
        var (leavingCharacters, invalidCharacters) = GetLeavingObjects(aoeObject, aoeState.characterInRange, aoeState.radius);
        
        // 移除无效引用
        foreach (var invalid in invalidCharacters)
        {
            aoeState.characterInRange.Remove(invalid);
        }

        // 触发离开事件
        if (leavingCharacters.Count > 0)
        {
            aoeState.model.onChaLeave?.Invoke(aoeObject, leavingCharacters);
            foreach (var leaving in leavingCharacters)
            {
                aoeState.characterInRange.Remove(leaving);
            }
        }

        // 处理进入的角色
        var enteringCharacters = GetEnteringObjects(aoeObject, characters, aoeState.characterInRange, aoeState.radius);
        
        // 触发进入事件
        if (enteringCharacters.Count > 0)
        {
            aoeState.model.onChaEnter?.Invoke(aoeObject, enteringCharacters);
            foreach (var entering in enteringCharacters)
            {
                if (entering != null && 
                    entering.GetComponent<ChaState>() && 
                    !entering.GetComponent<ChaState>().dead)
                {
                    aoeState.characterInRange.Add(entering);
                }
            }
        }
    }

    /// <summary>
    /// 处理子弹的进入和离开
    /// </summary>
    private void ProcessBulletMovement(GameObject aoeObject, AoeState aoeState, GameObject[] bullets)
    {
        // 处理离开的子弹
        var (leavingBullets, invalidBullets) = GetLeavingObjects(aoeObject, aoeState.bulletInRange, aoeState.radius);
        
        // 移除无效引用
        foreach (var invalid in invalidBullets)
        {
            aoeState.bulletInRange.Remove(invalid);
        }

        // 触发离开事件
        if (leavingBullets.Count > 0)
        {
            aoeState.model.onBulletLeave?.Invoke(aoeObject, leavingBullets);
            foreach (var leaving in leavingBullets)
            {
                aoeState.bulletInRange.Remove(leaving);
            }
        }

        // 处理进入的子弹
        var enteringBullets = GetEnteringObjects(aoeObject, bullets, aoeState.bulletInRange, aoeState.radius);
        
        // 触发进入事件
        if (enteringBullets.Count > 0)
        {
            aoeState.model.onBulletEnter?.Invoke(aoeObject, enteringBullets);
            foreach (var entering in enteringBullets)
            {
                if (entering != null)
                {
                    aoeState.bulletInRange.Add(entering);
                }
            }
        }
    }

    /// <summary>
    /// 更新AOE生命周期
    /// </summary>
    private void UpdateAoeLifecycle(GameObject aoeObject, AoeState aoeState, float deltaTime)
    {
        aoeState.duration -= deltaTime;
        aoeState.timeElapsed += deltaTime;

        if (ShouldRemoveAoe(aoeState))
        {
            RemoveAoeEffect(aoeObject, aoeState);
            return;
        }

        // 触发时间间隔事件
        if (ShouldTriggerTick(aoeState))
        {
            aoeState.model.onTick?.Invoke(aoeObject);
        }
    }

    #region 辅助方法
    /// <summary>
    /// 检查对象是否在范围内
    /// </summary>
    private bool IsObjectInRange(GameObject source, GameObject target, float radius)
    {
        if (!target) return false;
        
        return Utils.InRange(
            source.transform.position.x, source.transform.position.z,
            target.transform.position.x, target.transform.position.z,
            radius
        );
    }

    /// <summary>
    /// 获取离开范围的对象
    /// </summary>
    private (List<GameObject> leaving, List<GameObject> invalid) GetLeavingObjects(
        GameObject source, 
        List<GameObject> objectsInRange, 
        float radius)
    {
        var leavingObjects = new List<GameObject>();
        var invalidObjects = new List<GameObject>();

        foreach (var obj in objectsInRange)
        {
            if (obj != null)
            {
                if (!IsObjectInRange(source, obj, radius))
                {
                    leavingObjects.Add(obj);
                }
            }
            else
            {
                invalidObjects.Add(obj);
            }
        }

        return (leavingObjects, invalidObjects);
    }

    /// <summary>
    /// 获取进入范围的对象
    /// </summary>
    private List<GameObject> GetEnteringObjects(
        GameObject source, 
        GameObject[] allObjects, 
        List<GameObject> objectsInRange, 
        float radius)
    {
        var enteringObjects = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj != null && 
                !objectsInRange.Contains(obj) && 
                IsObjectInRange(source, obj, radius))
            {
                enteringObjects.Add(obj);
            }
        }

        return enteringObjects;
    }

    /// <summary>
    /// 检查是否应该移除AOE效果
    /// </summary>
    private bool ShouldRemoveAoe(AoeState aoeState)
    {
        return aoeState.duration <= 0 || aoeState.HitObstacle();
    }

    /// <summary>
    /// 检查是否应该触发时间间隔事件
    /// </summary>
    private bool ShouldTriggerTick(AoeState aoeState)
    {
        return aoeState.model.tickTime > 0 && 
               aoeState.model.onTick != null &&
               Mathf.RoundToInt(aoeState.duration * 1000) % Mathf.RoundToInt(aoeState.model.tickTime * 1000) == 0;
    }

    /// <summary>
    /// 移除AOE效果
    /// </summary>
    private void RemoveAoeEffect(GameObject aoeObject, AoeState aoeState)
    {
        aoeState.model.onRemoved?.Invoke(aoeObject);
        Destroy(aoeObject);
    }
    #endregion
    #endregion
}