using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景变量和全局辅助方法类
/// 提供地图管理和游戏对象交互的静态方法
/// 作为游戏各系统之间的桥梁
/// </summary>
public class SceneVariants
{
    /// <summary>
    /// 当前地图信息
    /// </summary>
    public static MapInfo map;

    #region 地图生成

    /// <summary>
    /// 生成随机地图
    /// </summary>
    /// <param name="mapWidth">地图宽度</param>
    /// <param name="mapHeight">地图高度</param>
    /// <param name="waterline">水线高度（决定水和草地的比例）</param>
    public static void RandomMap(int mapWidth, int mapHeight, float waterline = 6.00f)
    {
        // 创建基本地形类型
        GridInfo grass = new GridInfo("Terrain/Grass");
        GridInfo water = new GridInfo("Terrain/Water", false);
        
        // 初始化地图网格数组
        GridInfo[,] mapGrids = new GridInfo[mapWidth, mapHeight];
        
        // 使用柏林噪声生成随机地形
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                // 生成随机值决定地形类型
                float noiseValue = Mathf.PerlinNoise(i / (float)mapWidth, j / (float)mapHeight) * Random.Range(10.00f, 20.00f);
                mapGrids[i, j] = (noiseValue <= waterline) ? water : grass;
            }
        }
        
        // 创建地图信息对象
        map = new MapInfo(mapGrids, Vector2.one);
    }

    #endregion

    #region 游戏对象访问

    /// <summary>
    /// 获取主角游戏对象
    /// </summary>
    /// <returns>主角游戏对象</returns>
    public static GameObject MainActor()
    {
        return GameObject.Find("GameManager").GetComponent<GameManager>().mainActor;
    }

    #endregion

    #region 子弹系统

    /// <summary>
    /// 创建子弹
    /// </summary>
    /// <param name="bulletLauncher">子弹发射器</param>
    public static void CreateBullet(BulletLauncher bulletLauncher)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateBullet(bulletLauncher);
    }

    /// <summary>
    /// 移除子弹
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    /// <param name="immediately">是否立即移除</param>
    public static void RemoveBullet(GameObject bullet, bool immediately = false)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveBullet(bullet, immediately);
    }

    #endregion

    #region AOE系统

    /// <summary>
    /// 创建区域效果
    /// </summary>
    /// <param name="aoeLauncher">区域效果发射器</param>
    public static void CreateAoE(AoeLauncher aoeLauncher)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateAoE(aoeLauncher);
    }

    /// <summary>
    /// 移除区域效果
    /// </summary>
    /// <param name="aoe">区域效果对象</param>
    /// <param name="immediately">是否立即移除</param>
    public static void RemoveAoE(GameObject aoe, bool immediately = false)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveAoE(aoe, immediately);
    }

    #endregion

    #region 时间线系统

    /// <summary>
    /// 创建时间线事件（使用模型）
    /// </summary>
    /// <param name="timelineModel">时间线模型</param>
    /// <param name="caster">施放者</param>
    /// <param name="source">源对象</param>
    public static void CreateTimeline(TimelineModel timelineModel, GameObject caster, object source)
    {
        GameObject.Find("GameManager").GetComponent<TimelineManager>().AddTimeline(timelineModel, caster, source);
    }

    /// <summary>
    /// 创建时间线事件（使用对象）
    /// </summary>
    /// <param name="timeline">时间线对象</param>
    public static void CreateTimeline(TimelineObj timeline)
    {
        GameObject.Find("GameManager").GetComponent<TimelineManager>().AddTimeline(timeline);
    }

    #endregion

    #region 视觉效果

    /// <summary>
    /// 创建视觉效果
    /// </summary>
    /// <param name="prefab">预制体路径</param>
    /// <param name="pos">位置</param>
    /// <param name="degree">角度</param>
    /// <param name="key">效果标识符</param>
    /// <param name="loop">是否循环播放</param>
    public static void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CreateSightEffect(prefab, pos, degree, key, loop);
    }

    /// <summary>
    /// 移除视觉效果
    /// </summary>
    /// <param name="key">效果标识符</param>
    public static void RemoveSightEffect(string key)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().RemoveSightEffect(key);
    }

    #endregion

    #region 伤害系统

    /// <summary>
    /// 创建伤害
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害数据</param>
    /// <param name="damageDegree">伤害系数</param>
    /// <param name="criticalRate">暴击率</param>
    /// <param name="tags">伤害标签</param>
    public static void CreateDamage(GameObject attacker, GameObject target, Damage damage, float damageDegree, float criticalRate, DamageInfoTag[] tags)
    {
        GameObject.Find("GameManager").GetComponent<DamageManager>().DoDamage(attacker, target, damage, damageDegree, criticalRate, tags);
    }

    #endregion

    #region 角色系统

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="prefab">预制体路径</param>
    /// <param name="side">阵营</param>
    /// <param name="pos">位置</param>
    /// <param name="baseProp">基础属性</param>
    /// <param name="degree">朝向角度</param>
    /// <param name="unitAnimInfo">动画信息名称</param>
    /// <param name="tags">角色标签</param>
    /// <returns>创建的角色对象</returns>
    public static GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null)
    {
        return GameObject.Find("GameManager").GetComponent<GameManager>().CreateCharacter(prefab, side, pos, baseProp, degree, unitAnimInfo, tags);
    }

    #endregion

    #region UI效果

    /// <summary>
    /// 在角色上方弹出数字
    /// </summary>
    /// <param name="cha">目标角色</param>
    /// <param name="value">数值</param>
    /// <param name="asHeal">是否为治疗</param>
    /// <param name="asCritical">是否为暴击</param>
    public static void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false)
    {
        GameObject.Find("Canvas").GetComponent<PopTextManager>().PopUpNumberOnCharacter(cha, value, asHeal, asCritical);
    }

    /// <summary>
    /// 在角色上方弹出文本
    /// </summary>
    /// <param name="cha">目标角色</param>
    /// <param name="text">文本内容</param>
    /// <param name="size">文本大小</param>
    public static void PopUpStringOnCharacter(GameObject cha, string text, int size = 30)
    {
        GameObject.Find("Canvas").GetComponent<PopTextManager>().PopUpStringOnCharacter(cha, text, size);
    }

    #endregion
}
