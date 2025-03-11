# 通用战斗系统（Generic Combat System）（待完成）

## 项目简介
本项目的目标是提供一个灵活且易于扩展的战斗系统核心，适配各类有本质相同点的游戏模式，如：
- MOBA游戏
- 塔防游戏
- Roguelike游戏
- FPS游戏
- 顶视角射击游戏

## 系统架构
通用战斗系统采用数据驱动、组件化的设计思想，主要由以下核心模块组成：

### 1. 核心战斗系统（Core）
- **管理器（Managers）**：
  - AoeManager：区域效果管理
  - BulletManager：子弹系统管理
  - DamageManager：伤害计算核心
  - TimelineManager：技能时间轴系统
  - BaseGameManager：游戏管理器基类
  - GameModeManager：游戏模式管理

- **接口（Interfaces）**：
  - IGameMode：游戏模式接口

- **数据（Data）**：
  - 通用数据结构
  - 设计师脚本和表格

- **组件（Components）**：
  - 角色状态和属性
  - 子弹和AOE效果
  - 通用动画和视觉效果

### 2. 游戏模式（GameModes）
每种游戏模式包含自己特有的实现，但共享核心战斗系统：

- **......**：
  - ......

- **Roguelike（已初始化框架）**：
  - 管理器：RoguelikeManager
  - 地牢生成系统（待实现）
  - 特有输入控制（待实现）

## 目录结构
```
Assets/Scripts/
  ├─Core/ （核心战斗系统）
  │  ├─Managers/ （核心管理器）
  │  ├─Interfaces/ （接口定义）
  │  ├─Data/ （数据定义）
  │  └─Components/ （核心组件）
  │
  └─GameModes/ （游戏模式）
     ├─......
     └─Roguelike/ （Roguelike模式）
```

## 扩展到新游戏模式
要添加一个新的游戏模式，需要完成以下步骤：

1. 在GameModes目录下创建新的游戏模式文件夹
2. 继承BaseGameManager创建特定的游戏模式管理器
3. 实现IGameMode接口的方法：
4. 创建特定于该游戏模式的数据表、控制器和UI
