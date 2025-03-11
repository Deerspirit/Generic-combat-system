using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Core.Managers
{
    /// <summary>
    /// 基础游戏管理器：所有特定游戏模式管理器的抽象基类
    /// 包含通用战斗系统的核心功能，但将特定游戏模式的实现委托给子类
    /// </summary>
    public abstract class BaseGameManager : MonoBehaviour, IGameMode
    {
        #region 字段
        // 主角引用
        protected GameObject mainCharacter;

        // 游戏对象根节点
        protected GameObject root;

        // 视觉效果字典
        protected Dictionary<string, GameObject> sightEffect = new Dictionary<string, GameObject>();
        #endregion

        #region Unity生命周期
        protected virtual void Start()
        {
            // 初始化游戏基础设置
            root = GameObject.Find("GameObjectLayer");
            if (!root)
            {
                root = new GameObject("GameObjectLayer");
            }

            // 执行游戏模式初始化流程
            InitializeGameMode();
            mainCharacter = CreateMainCharacter();
            SetupCamera(mainCharacter);
            SetupInput(mainCharacter);
            
            Debug.Log($"游戏模式 [{GetGameModeName()}] 已初始化");
        }

        protected virtual void FixedUpdate()
        {
            CleanupSightEffects();
        }
        #endregion

        #region IGameMode实现
        // 这些方法需要由具体游戏模式实现
        public abstract void InitializeGameMode();
        public abstract GameObject CreateMainCharacter();
        public abstract void SetupCamera(GameObject mainCharacter);
        public abstract void SetupInput(GameObject mainCharacter);
        public abstract string GetGameModeName();
        #endregion

        #region 战斗系统核心方法
        /// <summary>
        /// 创建子弹
        /// </summary>
        public virtual void CreateBullet(BulletLauncher bulletLauncher)
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
        public virtual void RemoveBullet(GameObject bullet, bool immediately = false)
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
        public virtual void CreateAoE(AoeLauncher aoeLauncher)
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
        public virtual void RemoveAoE(GameObject aoe, bool immediately = false)
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

        /// <summary>
        /// 创建视觉效果
        /// </summary>
        public virtual void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false)
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
        public virtual void RemoveSightEffect(string key)
        {
            if (!sightEffect.ContainsKey(key)) return;
            
            Destroy(sightEffect[key]);
            sightEffect.Remove(key);
        }

        /// <summary>
        /// 清理无效的视觉效果
        /// </summary>
        protected virtual void CleanupSightEffects()
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
        protected virtual GameObject CreateFromPrefab(string prefabPath, Vector3 position = new Vector3(), float rotation = 0.00f)
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
        public virtual GameObject CreateCharacter(
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
}
