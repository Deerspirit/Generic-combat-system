using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// 伤害管理器：处理游戏中所有的伤害计算、Buff效果和治疗
/// </summary>
public class DamageManager : MonoBehaviour
{
    #region 字段
    private List<DamageInfo> damageInfos = new List<DamageInfo>();
    #endregion

    #region Unity生命周期
    private void FixedUpdate()
    {
        ProcessDamageQueue();
    }
    #endregion

    #region 伤害处理
    /// <summary>
    /// 处理伤害队列
    /// </summary>
    private void ProcessDamageQueue()
    {
        while (damageInfos.Count > 0)
        {
            DealWithDamage(damageInfos[0]);
            damageInfos.RemoveAt(0);
        }
    }

    /// <summary>
    /// 处理单个伤害信息
    /// </summary>
    private void DealWithDamage(DamageInfo dInfo)
    {
        if (!ValidateDamageInfo(dInfo)) return;

        ChaState defenderChaState = dInfo.defender.GetComponent<ChaState>();
        ChaState attackerChaState = dInfo.attacker?.GetComponent<ChaState>();

        // 处理攻击者的Buff效果
        ProcessAttackerBuffs(attackerChaState, ref dInfo);

        // 处理防御者的Buff效果
        ProcessDefenderBuffs(defenderChaState, ref dInfo);

        // 处理击杀事件
        ProcessKillEvent(attackerChaState, defenderChaState, dInfo);

        // 应用伤害或治疗
        ApplyDamageOrHeal(defenderChaState, dInfo);

        // 添加新的Buff效果
        ApplyNewBuffs(attackerChaState, defenderChaState, dInfo);
    }

    /// <summary>
    /// 验证伤害信息的有效性
    /// </summary>
    private bool ValidateDamageInfo(DamageInfo dInfo)
    {
        if (!dInfo.defender) return false;
        
        ChaState defenderChaState = dInfo.defender.GetComponent<ChaState>();
        if (!defenderChaState || defenderChaState.dead) return false;
        
        return true;
    }

    /// <summary>
    /// 处理攻击者的Buff效果
    /// </summary>
    private void ProcessAttackerBuffs(ChaState attackerChaState, ref DamageInfo dInfo)
    {
        if (attackerChaState == null) return;

        foreach (var buff in attackerChaState.buffs)
        {
            buff.model.onHit?.Invoke(buff, ref dInfo, dInfo.defender);
        }
    }

    /// <summary>
    /// 处理防御者的Buff效果
    /// </summary>
    private void ProcessDefenderBuffs(ChaState defenderChaState, ref DamageInfo dInfo)
    {
        foreach (var buff in defenderChaState.buffs)
        {
            buff.model.onBeHurt?.Invoke(buff, ref dInfo, dInfo.attacker);
        }
    }

    /// <summary>
    /// 处理击杀事件
    /// </summary>
    private void ProcessKillEvent(ChaState attackerChaState, ChaState defenderChaState, DamageInfo dInfo)
    {
        if (!defenderChaState.CanBeKilledByDamageInfo(dInfo)) return;

        // 处理攻击者的击杀效果
        if (attackerChaState != null)
        {
            foreach (var buff in attackerChaState.buffs)
            {
                buff.model.onKill?.Invoke(buff, dInfo, dInfo.defender);
            }
        }

        // 处理防御者的被击杀效果
        foreach (var buff in defenderChaState.buffs)
        {
            buff.model.onBeKilled?.Invoke(buff, dInfo, dInfo.attacker);
        }
    }

    /// <summary>
    /// 应用伤害或治疗效果
    /// </summary>
    private void ApplyDamageOrHeal(ChaState defenderChaState, DamageInfo dInfo)
    {
        bool isHeal = dInfo.isHeal();
        int damageValue = dInfo.DamageValue(isHeal);

        if (isHeal || defenderChaState.immuneTime <= 0)
        {
            // 播放受伤动画
            if (dInfo.requireDoHurt() && !defenderChaState.CanBeKilledByDamageInfo(dInfo))
            {
                PlayHurtAnimation(defenderChaState);
            }

            // 修改资源值（生命值等）
            defenderChaState.ModResource(new ChaResource(-damageValue));

            // 显示伤害/治疗数字
            SceneVariants.PopUpNumberOnCharacter(dInfo.defender, Mathf.Abs(damageValue), isHeal);
        }
    }

    /// <summary>
    /// 播放受伤动画
    /// </summary>
    private void PlayHurtAnimation(ChaState defenderChaState)
    {
        UnitAnim unitAnim = defenderChaState.GetComponent<UnitAnim>();
        unitAnim?.Play("Hurt");
    }

    /// <summary>
    /// 应用新的Buff效果
    /// </summary>
    private void ApplyNewBuffs(ChaState attackerChaState, ChaState defenderChaState, DamageInfo dInfo)
    {
        foreach (var buffInfo in dInfo.addBuffs)
        {
            GameObject targetObj = buffInfo.target;
            ChaState targetChaState = targetObj.Equals(dInfo.attacker) ? attackerChaState : defenderChaState;

            if (targetChaState != null && !targetChaState.dead)
            {
                targetChaState.AddBuff(buffInfo);
            }
        }
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 造成伤害
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害数值</param>
    /// <param name="damageDegree">伤害程度</param>
    /// <param name="criticalRate">暴击率</param>
    /// <param name="tags">伤害标签</param>
    public void DoDamage(
        GameObject attacker, 
        GameObject target, 
        Damage damage, 
        float damageDegree, 
        float criticalRate, 
        DamageInfoTag[] tags)
    {
        damageInfos.Add(new DamageInfo(
            attacker, 
            target, 
            damage, 
            damageDegree, 
            criticalRate, 
            tags
        ));
    }
    #endregion
}
