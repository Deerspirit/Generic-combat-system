using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 设计师脚本命名空间：包含所有可由设计师直接配置和使用的公共脚本
/// </summary>
namespace DesignerScripts
{
    /// <summary>
    /// 通用脚本类：提供常用游戏计算和工具函数
    /// 可直接在各种设计师配置中引用
    /// </summary>
    public class CommonScripts
    {
        /// <summary>
        /// 计算最终伤害值
        /// </summary>
        /// <param name="damageInfo">伤害信息对象</param>
        /// <param name="asHeal">是否作为治疗计算（默认为false）</param>
        /// <returns>计算后的最终伤害/治疗数值（向上取整）</returns>
        public static int DamageValue(DamageInfo damageInfo, bool asHeal = false)
        {
            // 根据暴击率计算是否触发暴击
            bool isCritical = Random.Range(0.00f, 1.00f) <= damageInfo.criticalRate;
            
            // 计算最终伤害值，暴击时伤害乘以1.8
            float baseDamage = damageInfo.damage.Overall(asHeal);
            float finalDamage = baseDamage * (isCritical ? 1.80f : 1.00f);
            
            // 向上取整，确保最小伤害为1
            return Mathf.CeilToInt(finalDamage);
        }
    }
}
