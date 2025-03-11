using UnityEngine;
using Core.Managers;

namespace GameModes.Roguelike.Managers
{
    /// <summary>
    /// Roguelike游戏模式管理器：实现Roguelike游戏的特定功能
    /// </summary>
    public class RoguelikeManager : BaseGameManager
    {
        #region Roguelike特有字段
        [SerializeField]
        private string playerPrefab = "RogueHero";
        
        [SerializeField]
        private int dungeonWidth = 20;
        
        [SerializeField]
        private int dungeonHeight = 20;
        
        [SerializeField]
        private int roomCount = 5;
        #endregion

        #region IGameMode实现
        public override void InitializeGameMode()
        {
            // 初始化Roguelike特有的游戏设置
            Debug.Log("初始化Roguelike游戏模式 - 这里将生成随机地牢地图");
            // 这里应该有地牢地图生成的代码
            // GenerateDungeon(dungeonWidth, dungeonHeight, roomCount);
        }

        public override GameObject CreateMainCharacter()
        {
            // 创建Roguelike主角
            Debug.Log("创建Roguelike主角");
            
            // 示例：在中心位置创建主角
            Vector3 playerPos = new Vector3(dungeonWidth / 2, 0, dungeonHeight / 2);
            
            mainCharacter = CreateCharacter(
                playerPrefab, 
                1, 
                playerPos, 
                new ChaProperty(150, 500, 800, 60, 150), 
                0
            );
            
            // 初始化初始技能和装备
            // InitializeRoguelikeCharacter(mainCharacter);
            
            return mainCharacter;
        }

        public override void SetupCamera(GameObject mainCharacter)
        {
            Debug.Log("设置Roguelike相机视角");
            
            // 这里将设置适合Roguelike游戏的相机系统
            // 例如：俯视角、2.5D等
            
            // 示例：简单地使用现有摄像机先跟随主角
            var mainCamera = Camera.main;
            if (mainCamera)
            {
                mainCamera.transform.position = new Vector3(
                    mainCharacter.transform.position.x,
                    10, // 高度
                    mainCharacter.transform.position.z - 5 // 偏移
                );
                
                mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0); // 45度俯视角
                
                // 如果有跟随脚本，可以使用它，否则需要创建一个适合Roguelike的跟随脚本
                var camFollow = mainCamera.GetComponent<CamFollow>();
                if (camFollow)
                {
                    camFollow.SetFollowCharacter(mainCharacter);
                }
            }
        }

        public override void SetupInput(GameObject mainCharacter)
        {
            Debug.Log("设置Roguelike输入控制");
            
            // 这里将设置适合Roguelike游戏的输入控制
            // 例如：点击移动、技能快捷键等
            
            // 示例：暂时还没有实现Roguelike专用控制器，先使用现有控制器
            var playerController = mainCharacter.AddComponent<PlayerController>();
            if (playerController)
            {
                playerController.mainCamera = Camera.main;
            }
        }

        public override string GetGameModeName()
        {
            return "Roguelike";
        }
        #endregion

        #region Roguelike特有方法
        /// <summary>
        /// 生成随机地牢
        /// </summary>
        private void GenerateDungeon(int width, int height, int rooms)
        {
            // 这里将实现Roguelike地牢生成算法
            Debug.Log($"生成地牢 - 宽度: {width}, 高度: {height}, 房间数: {rooms}");
            
            // 1. 创建基础地图网格
            // 2. 生成随机房间
            // 3. 连接房间形成通道
            // 4. 放置门、宝箱、敌人等
        }
        
        /// <summary>
        /// 初始化Roguelike角色
        /// </summary>
        private void InitializeRoguelikeCharacter(GameObject character)
        {
            Debug.Log("初始化Roguelike角色");
            
            // 1. 添加初始技能
            // 2. 添加初始装备
            // 3. 设置初始属性和能力
        }
        #endregion
    }
}
