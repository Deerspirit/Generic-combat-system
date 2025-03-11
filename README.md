# 通用战斗系统（Generic Combat System）

## 项目简介
这是一个基于Unity开发的高度可扩展的通用战斗系统框架。本项目的目标是提供一个灵活且易于扩展的战斗系统核心，使开发者能够快速构建各类游戏，如：
- MOBA游戏
- 塔防游戏
- Roguelike游戏
- FPS游戏
- 顶视角射击游戏（当前已实现）

系统的设计理念是"一次构建，多处使用"，通过合理的抽象和模块化设计，使得开发者可以在同一项目中实现多种不同类型的游戏玩法，而无需重复构建基础系统。

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

- **顶视角射击（TopDownShooter）**：
  - 管理器：TopDownGameManager、MobSpawnManager
  - 控制器：PlayController、SimpleAI
  - 相机系统：CamFollow
  - 特有UI和效果

- **Roguelike（已初始化框架）**：
  - 管理器：RoguelikeManager
  - 地牢生成系统（待实现）
  - 特有输入控制（待实现）

## 新的目录结构
```
Assets/Scripts/
  ├─Core/ （核心战斗系统）
  │  ├─Managers/ （核心管理器）
  │  ├─Interfaces/ （接口定义）
  │  ├─Data/ （数据定义）
  │  └─Components/ （核心组件）
  │
  └─GameModes/ （游戏模式）
     ├─TopDownShooter/ （顶视角射击）
     └─Roguelike/ （Roguelike模式）
```

## 扩展到新游戏模式
要添加一个新的游戏模式，只需要完成以下步骤：

1. 在GameModes目录下创建新的游戏模式文件夹
2. 继承BaseGameManager创建特定的游戏模式管理器
3. 实现IGameMode接口的方法：
   - InitializeGameMode：初始化游戏模式特有内容
   - CreateMainCharacter：创建适合该模式的主角
   - SetupCamera：设置适合该模式的相机系统
   - SetupInput：设置适合该模式的输入控制
4. 创建特定于该游戏模式的数据表、控制器和UI

## 使用方法
### 基本设置
1. 场景中添加GameModeManager对象
2. 添加所有核心管理器：AoeManager, BulletManager, DamageManager, TimelineManager
3. 添加需要的游戏模式管理器（如TopDownGameManager, RoguelikeManager）
4. 启动游戏，GameModeManager会自动管理游戏模式的激活与切换

### 游戏模式切换
可以通过以下方式切换游戏模式：
```csharp
// 在任何脚本中：
GameModeManager.Instance.SwitchToGameMode("TopDownShooter");
// 或
GameModeManager.Instance.SwitchToGameMode("Roguelike");
```

### 创建新技能
1. 在GameData/DesignerTables/Skill.cs中定义新技能
2. 设置技能的Timeline（技能释放流程）
3. 配置必要的子弹或AOE效果
4. 使用ChaState.LearnSkill方法让角色学习技能

### 自定义子弹和AOE效果
1. 在相关数据表中定义属性
2. 配置移动方式、碰撞逻辑和效果
3. 在Timeline中使用创建事件
