using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// 游戏模式接口：定义所有游戏模式必须实现的功能
    /// </summary>
    public interface IGameMode
    {
        /// <summary>
        /// 初始化游戏模式
        /// </summary>
        void InitializeGameMode();

        /// <summary>
        /// 创建主角
        /// </summary>
        /// <returns>创建的主角游戏对象</returns>
        GameObject CreateMainCharacter();

        /// <summary>
        /// 设置摄像机
        /// </summary>
        /// <param name="mainCharacter">主角游戏对象</param>
        void SetupCamera(GameObject mainCharacter);

        /// <summary>
        /// 设置输入控制
        /// </summary>
        /// <param name="mainCharacter">主角游戏对象</param>
        void SetupInput(GameObject mainCharacter);

        /// <summary>
        /// 获取游戏模式名称
        /// </summary>
        string GetGameModeName();
    }
}
