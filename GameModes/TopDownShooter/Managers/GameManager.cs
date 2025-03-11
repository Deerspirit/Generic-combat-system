using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏管理器：负责管理游戏的核心功能，包括角色创建、地图生成、战斗系统等
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 字段
    // 主角引用
    public GameObject mainActor { get { return mainCharacter; } }
    private GameObject mainCharacter;

    // 游戏对象根节点
    private GameObject root;

    // 视觉效果字典
    private Dictionary<string, GameObject> sightEffect = new Dictionary<string, GameObject>();
    #endregion

    #region Unity生命周期
    void Start()
    {
        InitializeGame();
        CreateMainCharacter();
        SetupCamera();
        SetupUI();
        InitializePlayerSkills();
    }

    private void FixedUpdate()
    {
        CleanupSightEffects();
    }
    #endregion

    #region 初始化方法
    /// <summary>
    /// 初始化游戏基础设置
    /// </summary>
    private void InitializeGame()
    {
        root = GameObject.Find("GameObjectLayer");
        SceneVariants.RandomMap(Random.Range(10, 15), Random.Range(10, 15));
        CreateMapGameObjects();
    }

    /// <summary>
    /// 创建并设置主角
    /// </summary>
    private void CreateMainCharacter()
    {
        Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(
            new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())
        );
        
        mainCharacter = CreateCharacter(
            "FemaleGunner", 
            1, 
            playerPos, 
            new ChaProperty(100, Random.Range(5000, 7000), 600, Random.Range(50, 70)), 
            0
        );
        
        mainCharacter.AddComponent<PlayerController>().mainCamera = Camera.main;
    }

    /// <summary>
    /// 设置摄像机跟随
    /// </summary>
    private void SetupCamera()
    {
        GameObject.Find("Main Camera").GetComponent<CamFollow>().SetFollowCharacter(mainCharacter);
    }

    /// <summary>
    /// 设置UI系统
    /// </summary>
    private void SetupUI()
    {
        GameObject.Find("PlayerHP").GetComponent<PlayerStateListener>().playerGameObject = mainCharacter;
    }

    /// <summary>
    /// 初始化玩家技能
    /// </summary>
    private void InitializePlayerSkills()
    {
        ChaState mcs = mainCharacter.GetComponent<ChaState>();
        var skillData = DesingerTables.Skill.data;
        
        string[] initialSkills = new string[] {
            "fire", "roll", "spaceMonkeyBall", "homingMissle",
            "cloakBoomerang", "teleportBullet", "grenade", "explosiveBarrel"
        };

        foreach (string skillName in initialSkills)
        {
            mcs.LearnSkill(skillData[skillName]);
        }
    }
    #endregion

    #region 地图生成
    /// <summary>
    /// 创建地图游戏对象
    /// </summary>
    private void CreateMapGameObjects()
    {
        // 清理现有地图
        CleanupExistingMap();
        
        // 生成新地图
        for (var i = 0; i < SceneVariants.map.MapWidth(); i++)
        {
            for (var j = 0; j < SceneVariants.map.MapHeight(); j++)
            {
                CreateFromPrefab(
                    SceneVariants.map.grid[i, j].prefabPath, 
                    new Vector3(i, 0, j)
                );
            }
        }
    }

    /// <summary>
    /// 清理现有地图
    /// </summary>
    private void CleanupExistingMap()
    {
        GameObject[] mt = GameObject.FindGameObjectsWithTag("MapTile");
        foreach (var tile in mt)
        {
            Destroy(tile);
        }
    }
    #endregion

    #region 战斗系统
    /// <summary>
    /// 创建子弹
    /// </summary>
    public void CreateBullet(BulletLauncher bulletLauncher)
    {
        GameObject bulletObj = Instantiate(
            Resources.Load<GameObject>("Prefabs/Bullet/BulletObj"),
            bulletLauncher.firePosition,
            Quaternion.identity,
            root.transform
        );

        // 设置子弹旋转
        bulletObj.transform.RotateAround(
            bulletObj.transform.position, 
            Vector3.up, 
            bulletLauncher.fireDegree
        );

        // 初始化子弹状态
        bulletObj.GetComponent<BulletState>().InitByBulletLauncher(
            bulletLauncher,
            GameObject.FindGameObjectsWithTag("Character")
        );
    }

    /// <summary>
    /// 移除子弹
    /// </summary>
    public void RemoveBullet(GameObject bullet, bool immediately = false)
    {
        if (!bullet) return;
        
        BulletState bulletState = bullet.GetComponent<BulletState>();
        if (!bulletState) return;
        
        bulletState.duration = 0;
        
        if (immediately)
        {
            bulletState.model.onRemoved?.Invoke(bullet);
            Destroy(bullet);
        }
    }

    /// <summary>
    /// 创建AOE效果
    /// </summary>
    public void CreateAoE(AoeLauncher aoeLauncher)
    {
        GameObject aoeObj = Instantiate(
            Resources.Load<GameObject>("Prefabs/Effect/AoeObj"),
            aoeLauncher.position,
            Quaternion.identity,
            root.transform
        );

        aoeObj.GetComponent<AoeState>().InitByAoeLauncher(aoeLauncher);
    }

    /// <summary>
    /// 移除AOE效果
    /// </summary>
    public void RemoveAoE(GameObject aoe, bool immediately = false)
    {
        if (!aoe) return;
        
        AoeState aoeState = aoe.GetComponent<AoeState>();
        if (!aoeState) return;
        
        aoeState.duration = 0;
        
        if (immediately)
        {
            aoeState.model.onRemoved?.Invoke(aoe);
            Destroy(aoe);
        }
    }
    #endregion

    #region 视觉效果
    /// <summary>
    /// 创建视觉效果
    /// </summary>
    public void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false)
    {
        if (!string.IsNullOrEmpty(key) && sightEffect.ContainsKey(key)) return;

        GameObject effectGO = Instantiate(
            Resources.Load<GameObject>("Prefabs/" + prefab),
            pos,
            Quaternion.identity,
            this.gameObject.transform
        );

        effectGO.transform.RotateAround(effectGO.transform.position, Vector3.up, degree);
        
        if (!effectGO) return;
        
        SightEffect se = effectGO.GetComponent<SightEffect>();
        if (!se)
        {
            Destroy(effectGO);
            return;
        }

        if (!loop)
        {
            effectGO.AddComponent<UnitRemover>().duration = se.duration;
        }

        if (!string.IsNullOrEmpty(key))
        {
            sightEffect.Add(key, effectGO);
        }
    }

    /// <summary>
    /// 移除视觉效果
    /// </summary>
    public void RemoveSightEffect(string key)
    {
        if (!sightEffect.ContainsKey(key)) return;
        
        Destroy(sightEffect[key]);
        sightEffect.Remove(key);
    }

    /// <summary>
    /// 清理无效的视觉效果
    /// </summary>
    private void CleanupSightEffects()
    {
        List<string> toRemoveKey = new List<string>();
        
        foreach (var se in sightEffect)
        {
            if (se.Value == null) toRemoveKey.Add(se.Key);
        }
        
        foreach (var key in toRemoveKey)
        {
            sightEffect.Remove(key);
        }
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 从预制体创建游戏对象
    /// </summary>
    private GameObject CreateFromPrefab(string prefabPath, Vector3 position = new Vector3(), float rotation = 0.00f)
    {
        GameObject go = Instantiate(
            Resources.Load<GameObject>("Prefabs/" + prefabPath),
            position,
            Quaternion.identity
        );

        if (rotation != 0)
        {
            go.transform.Rotate(new Vector3(0, rotation, 0));
        }

        go.transform.SetParent(root.transform);
        return go;
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public GameObject CreateCharacter(
        string prefab, 
        int side, 
        Vector3 pos, 
        ChaProperty baseProp, 
        float degree, 
        string unitAnimInfo = "Default_Gunner", 
        string[] tags = null)
    {
        // 创建角色对象
        GameObject chaObj = CreateFromPrefab("Character/CharacterObj");
        
        // 设置角色状态
        ChaState cs = chaObj.GetComponent<ChaState>();
        if (cs)
        {
            cs.InitBaseProp(baseProp);
            cs.side = side;
            
            // 设置动画信息
            Dictionary<string, AnimInfo> aInfo = new Dictionary<string, AnimInfo>();
            if (!string.IsNullOrEmpty(unitAnimInfo) && DesingerTables.UnitAnimInfo.data.ContainsKey(unitAnimInfo))
            {
                aInfo = DesingerTables.UnitAnimInfo.data[unitAnimInfo];
            }
            
            cs.SetView(CreateFromPrefab("Character/" + prefab), aInfo);
            
            if (tags != null) cs.tags = tags;
        }

        // 设置位置和旋转
        chaObj.transform.position = pos;
        chaObj.transform.RotateAround(chaObj.transform.position, Vector3.up, degree);
        
        return chaObj;
    }
    #endregion
}
