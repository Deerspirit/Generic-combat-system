using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹管理器：处理游戏中所有子弹的移动、碰撞和生命周期
/// </summary>
public class BulletManager : MonoBehaviour
{
    #region Unity生命周期
    /// <summary>
    /// 每帧固定更新，处理所有子弹的逻辑
    /// </summary>
    private void FixedUpdate()
    {
        // 获取所有子弹和角色
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        
        // 如果没有子弹或角色，直接返回
        if (bullets.Length <= 0 || characters.Length <= 0)
            return;

        float deltaTime = Time.fixedDeltaTime;

        // 处理每一个子弹
        foreach (var bullet in bullets)
        {
            ProcessBullet(bullet, characters, deltaTime);
        }
    }
    #endregion

    #region 子弹处理
    /// <summary>
    /// 处理单个子弹的逻辑
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="characters">场景中的角色</param>
    /// <param name="deltaTime">时间增量</param>
    private void ProcessBullet(GameObject bullet, GameObject[] characters, float deltaTime)
    {
        // 获取子弹状态
        BulletState bulletState = bullet.GetComponent<BulletState>();
        if (bulletState == null || bulletState.hp <= 0)
            return;

        // 处理初始创建
        if (bulletState.timeElapsed <= 0 && bulletState.model.onCreate != null)
        {
            bulletState.model.onCreate(bullet);
        }

        // 清理过期的命中记录
        CleanupHitRecords(bulletState);

        // 更新子弹移动
        UpdateBulletMovement(bulletState);

        // 处理命中延迟
        if (bulletState.canHitAfterCreated > 0)
        {
            bulletState.canHitAfterCreated -= deltaTime;
        }
        else
        {
            // 检测子弹碰撞
            ProcessBulletCollision(bullet, bulletState, characters);
        }

        // 更新子弹生命周期
        UpdateBulletLifecycle(bullet, bulletState, deltaTime);
    }

    /// <summary>
    /// 清理过期的命中记录
    /// </summary>
    /// <param name="bulletState">子弹状态</param>
    private void CleanupHitRecords(BulletState bulletState)
    {
        int index = 0;
        while (index < bulletState.hitRecords.Count)
        {
            if (bulletState.hitRecords[index].timeToCanHit <= 0 || 
                bulletState.hitRecords[index].target == null)
            {
                bulletState.hitRecords.RemoveAt(index);
            }
            else
            {
                index++;
            }
        }
    }

    /// <summary>
    /// 更新子弹移动
    /// </summary>
    /// <param name="bulletState">子弹状态</param>
    private void UpdateBulletMovement(BulletState bulletState)
    {
        Vector3 moveForce = bulletState.tween == null 
            ? Vector3.forward 
            : bulletState.tween(bulletState.timeElapsed, bulletState.gameObject, bulletState.followingTarget);
            
        bulletState.SetMoveForce(moveForce);
    }

    /// <summary>
    /// 处理子弹与角色的碰撞
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="bulletState">子弹状态</param>
    /// <param name="characters">场景中的角色</param>
    private void ProcessBulletCollision(GameObject bullet, BulletState bulletState, GameObject[] characters)
    {
        float bulletRadius = bulletState.model.radius;
        int bulletSide = GetBulletSide(bulletState);

        foreach (var character in characters)
        {
            // 检查是否可以命中该角色
            if (!bulletState.CanHit(character))
                continue;

            // 获取角色状态
            ChaState characterState = character.GetComponent<ChaState>();
            if (characterState == null || characterState.dead || characterState.immuneTime > 0)
                continue;

            // 检查阵营关系
            if ((bulletState.model.hitAlly == false && bulletSide == characterState.side) ||
                (bulletState.model.hitFoe == false && bulletSide != characterState.side))
                continue;

            // 检查碰撞
            if (IsColliding(bullet, bulletRadius, character, characterState.property.hitRadius))
            {
                // 减少子弹生命值
                bulletState.hp--;

                // 触发命中事件
                bulletState.model.onHit?.Invoke(bullet, character);

                // 处理命中后的子弹
                if (bulletState.hp > 0)
                {
                    bulletState.AddHitRecord(character);
                }
                else
                {
                    Object.Destroy(bullet);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 获取子弹的阵营
    /// </summary>
    /// <param name="bulletState">子弹状态</param>
    /// <returns>子弹阵营，-1表示无阵营</returns>
    private int GetBulletSide(BulletState bulletState)
    {
        if (bulletState.caster)
        {
            ChaState casterState = bulletState.caster.GetComponent<ChaState>();
            if (casterState)
            {
                return casterState.side;
            }
        }
        return -1;
    }

    /// <summary>
    /// 检查是否发生碰撞
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="bulletRadius">子弹半径</param>
    /// <param name="character">角色对象</param>
    /// <param name="characterRadius">角色碰撞半径</param>
    /// <returns>是否碰撞</returns>
    private bool IsColliding(GameObject bullet, float bulletRadius, GameObject character, float characterRadius)
    {
        Vector3 distance = bullet.transform.position - character.transform.position;
        float distanceSquared = distance.x * distance.x + distance.z * distance.z;
        float radiusSum = bulletRadius + characterRadius;
        
        return distanceSquared <= radiusSum * radiusSum;
    }

    /// <summary>
    /// 更新子弹生命周期
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="bulletState">子弹状态</param>
    /// <param name="deltaTime">时间增量</param>
    private void UpdateBulletLifecycle(GameObject bullet, BulletState bulletState, float deltaTime)
    {
        bulletState.duration -= deltaTime;
        bulletState.timeElapsed += deltaTime;
        
        // 检查是否需要移除子弹
        if (bulletState.duration <= 0 || bulletState.HitObstacle())
        {
            bulletState.model.onRemoved?.Invoke(bullet);
            Object.Destroy(bullet);
        }
    }
    #endregion
}
