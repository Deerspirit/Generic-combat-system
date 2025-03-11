using UnityEngine;
using Core.Managers;
using System.Collections.Generic;

namespace GameModes.TopDownShooter.Managers
{
    /// <summary>
    /// 顶视角射击游戏模式管理器：实现顶视角射击游戏的特定功能
    /// </summary>
    public class TopDownGameManager : BaseGameManager
    {
        #region 顶视角特有字段
        [SerializeField]
        private string playerPrefab = "FemaleGunner";
        
        // 怪物生成管理器
        private MobSpawnManager mobSpawnManager;
        #endregion

        #region IGameMode实现
        public override void InitializeGameMode()
        {
            // 初始化顶视角特有的游戏设置
            SceneVariants.RandomMap(Random.Range(10, 15), Random.Range(10, 15));
            CreateMapGameObjects();
            
            // 获取怪物生成管理器
            mobSpawnManager = GetComponent<MobSpawnManager>();
            if (mobSpawnManager)
            {
                mobSpawnManager.enabled = true;
            }
        }

        public override GameObject CreateMainCharacter()
        {
            // 在随机位置创建主角
            Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(
                new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight())
            );
            
            mainCharacter = CreateCharacter(
                playerPrefab, 
                1, 
                playerPos, 
                new ChaProperty(100, Random.Range(5000, 7000), 600, Random.Range(50, 70)), 
                0
            );
            
            // 初始化玩家技能
            InitializePlayerSkills();
            
            return mainCharacter;
        }

        public override void SetupCamera(GameObject mainCharacter)
        {
            var mainCamera = GameObject.Find("Main Camera");
            if (mainCamera)
            {
                var camFollow = mainCamera.GetComponent<CamFollow>();
                if (camFollow)
                {
                    camFollow.SetFollowCharacter(mainCharacter);
                }
            }
        }

        public override void SetupInput(GameObject mainCharacter)
        {
            // 添加顶视角的玩家控制器
            var playerController = mainCharacter.AddComponent<PlayerController>();
            if (playerController)
            {
                playerController.mainCamera = Camera.main;
            }
            
            // 设置UI监听
            var playerHP = GameObject.Find("PlayerHP");
            if (playerHP)
            {
                var stateListener = playerHP.GetComponent<PlayerStateListener>();
                if (stateListener)
                {
                    stateListener.playerGameObject = mainCharacter;
                }
            }
        }

        public override string GetGameModeName()
        {
            return "TopDownShooter";
        }
        #endregion

        #region 顶视角特有方法
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
    }
} 