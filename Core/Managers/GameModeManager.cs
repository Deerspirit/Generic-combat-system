using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Core.Managers
{
    /// <summary>
    /// 游戏模式管理器：负责管理和切换不同的游戏模式
    /// 作为场景中的全局单例，用于协调不同游戏模式的激活与切换
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        #region 单例实现
        public static GameModeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region 字段和属性
        [SerializeField]
        private string defaultGameMode = "TopDownShooter";
        
        // 所有可用的游戏模式
        private Dictionary<string, GameObject> availableGameModes = new Dictionary<string, GameObject>();
        
        // 当前激活的游戏模式
        private IGameMode currentGameMode;
        
        // 当前激活的游戏模式名称
        public string CurrentGameModeName { get; private set; }
        #endregion

        #region Unity生命周期
        private void Start()
        {
            FindAndRegisterGameModes();
            SwitchToGameMode(defaultGameMode);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 切换到指定的游戏模式
        /// </summary>
        /// <param name="gameModeName">游戏模式名称</param>
        /// <returns>切换是否成功</returns>
        public bool SwitchToGameMode(string gameModeName)
        {
            if (!availableGameModes.ContainsKey(gameModeName))
            {
                Debug.LogError($"游戏模式 [{gameModeName}] 不存在或未注册");
                return false;
            }

            // 禁用当前所有游戏模式
            foreach (var mode in availableGameModes)
            {
                mode.Value.SetActive(mode.Key == gameModeName);
            }

            // 更新当前游戏模式引用
            currentGameMode = availableGameModes[gameModeName].GetComponent<IGameMode>();
            CurrentGameModeName = gameModeName;
            
            Debug.Log($"已切换到游戏模式: [{gameModeName}]");
            return true;
        }

        /// <summary>
        /// 注册新的游戏模式
        /// </summary>
        /// <param name="gameModeName">游戏模式名称</param>
        /// <param name="gameModeObject">游戏模式对象</param>
        public void RegisterGameMode(string gameModeName, GameObject gameModeObject)
        {
            if (availableGameModes.ContainsKey(gameModeName))
            {
                Debug.LogWarning($"游戏模式 [{gameModeName}] 已经注册，将被覆盖");
            }

            availableGameModes[gameModeName] = gameModeObject;
            gameModeObject.SetActive(false);
            
            Debug.Log($"已注册游戏模式: [{gameModeName}]");
        }
        
        /// <summary>
        /// 获取当前游戏模式
        /// </summary>
        /// <returns>当前游戏模式接口</returns>
        public IGameMode GetCurrentGameMode()
        {
            return currentGameMode;
        }
        
        /// <summary>
        /// 获取所有可用的游戏模式名称
        /// </summary>
        /// <returns>游戏模式名称列表</returns>
        public List<string> GetAvailableGameModes()
        {
            return new List<string>(availableGameModes.Keys);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 查找并注册场景中所有的游戏模式
        /// </summary>
        private void FindAndRegisterGameModes()
        {
            // 查找场景中的所有游戏模式实现
            var gameManagers = FindObjectsOfType<BaseGameManager>();
            foreach (var manager in gameManagers)
            {
                var gameModeInterface = manager as IGameMode;
                if (gameModeInterface != null)
                {
                    string name = gameModeInterface.GetGameModeName();
                    RegisterGameMode(name, manager.gameObject);
                }
            }
            
            Debug.Log($"已找到 {availableGameModes.Count} 个游戏模式");
        }
        #endregion
    }
}
