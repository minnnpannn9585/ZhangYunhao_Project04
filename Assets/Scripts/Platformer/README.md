# 2D 解谜跳跃平台游戏 —— 键位分配系统

本目录下的脚本实现了一个可运行的最小 Demo：基础物理/碰撞、角色移动、角色与地图的交互、
按键触发的屏幕文字，以及核心玩法——**键盘资源分配**（把动作在物理按键之间重新分配，部分
按键会因剧情/机关而失效并在切换界面里表现为“障碍”）。

打开 `Assets/Scenes/PlatformerDemo.unity` 即可试玩：
`A`/`D` 左右移动，`Space` 跳跃，`F` 与地图物体互动（告示牌/拉杆），`Tab` 打开键位分配面板。

## 目录结构与模块划分

```
Assets/Scripts/Platformer/
  Core/          与具体玩法无关的基础设施
    SingletonBehaviour.cs   泛型单例基类，供各 Manager 复用
    GameplayInputGate.cs    引用计数的“输入锁”，弹出式 UI 打开时顶层输入自动让位
  KeyBinding/    核心玩法：键位分配
    GameAction.cs           可分配的动作枚举（新增动作的唯一入口）
    KeyBindingManager.cs    单一数据源：动作↔按键映射、按键封锁状态、重新分配逻辑
    KeyboardLayoutSO.cs     键盘物理布局（行/列 → KeyCode），可配 ScriptableObject 资产
    KeyboardKeyBlocker.cs   触发器组件：进入区域时封锁/解封一组按键（“特殊情况导致按键失效”）
  Player/        玩家物理与输入
    PlayerMotor2D.cs        纯物理层：移动、跳跃、地面检测，不关心输入来自哪里
    PlayerController.cs     把 KeyBindingManager 的动作翻译成 Motor 指令
    PlayerInteractor.cs     跟踪范围内的可交互物体
  Interaction/   角色与地图的基本交互
    IInteractable.cs        通用交互接口
    Lever.cs / Door.cs      拉杆开关门的示例机关
    DialogueTrigger.cs      挂在 NPC/告示牌上，触发对话
  Dialogue/      按键显示文字
    DialogueData.cs         一段对话的数据（ScriptableObject，可在 Project 面板创建）
    DialogueUIController.cs 运行时自动搭建文本框 UI，每次按键推进一行
  UI/KeyRemap/   键位分配的界面与交互状态机
    KeyRemapMenuController.cs  状态机：关闭→选中键盘→选源键→选目标键
    KeyboardGridUI.cs          按 KeyboardLayoutSO 生成按键网格、处理光标移动/遮挡
    KeyButtonView.cs           单个按键格子的显示（正常/遮蔽变暗/光标/已选中源键）
```

依赖方向单一：`Player` 依赖 `KeyBinding` 和 `Core`；`UI` 依赖 `KeyBinding` 和 `Core`；
`Interaction`/`Dialogue` 相互独立，仅通过接口/引用被 `Player` 调用。没有任何模块反向依赖
`UI` 或 `Player`，方便以后单独替换或扩展某一层。

## 键位分配玩法是如何工作的

- `KeyBindingManager` 是唯一的数据源：`GetKey(action)`、`IsActionPressed/-Down/-Up(action)`、
  `IsKeyBlocked(key)`、`TryRebind(sourceKey, targetKey)`。**所有玩法脚本一律通过它读取输入**，
  不直接调用 `Input.GetKey`，因此重新分配或封锁按键会自动对全部功能生效，无需逐个特判。
- 封锁一个按键（`SetKeyBlocked`）不会清除绑定，而是让绑定在该键上的动作“哑火”，直到玩家把
  它挪到一个可用的键上——这正是“特殊情况导致按键失效，逼迫玩家重新分配”的机制来源。
  `KeyboardKeyBlocker` 是现成的触发器组件，可挂在任何危险区域上；一次性/剧情触发的封锁则直接
  调用 `KeyBindingManager.Instance.SetKeyBlocked(...)`。
- **菜单本身的操作键（打开菜单 `Tab`、导航方向键、确认 `Enter`、取消 `Esc`）是固定物理按键，
  不经过 `KeyBindingManager`**，因此无论玩家怎么重新分配、无论哪些键被封锁，都不可能把自己反
  锁在分配面板之外。这是刻意的设计约束，请不要把它们并入 `GameAction`。
- 分配面板的流程严格对应需求：`Tab` 打开 → 键盘整体高亮，`Enter` 选中键盘 → 方向键+`Enter`
  选中要挪动的“源键” → 方向键+`Enter` 选中“目标键”完成交换 → 回到选源键状态，可连续操作。
  `Esc` 逐级返回，直到关闭面板。
- 网格导航把封锁的键当作真正的障碍：`KeyboardGridUI.Move` 在目标格子越界/为空/被封锁时直接
  拒绝移动（就像撞墙），调用方不需要任何特殊分支。

## 扩展指南

- **新增可分配动作**：在 `GameAction.cs` 里加一个枚举值，在 `KeyBindingManager` 的
  `defaultBindings` 里给它一个默认键，然后在需要的地方调用
  `KeyBindingManager.Instance.IsActionDown(GameAction.你的新动作)`。UI 会自动识别并显示。
- **自定义键盘布局/手绘键盘图**：新建一个 `KeyboardLayoutSO` 资产（Assets ▸ Create ▸
  Platformer ▸ Key Binding ▸ Keyboard Layout），按你手绘图上的实际排列填格子；如果某个键在
  图上的位置/大小不是规则网格，勾选该格的 `overrideRect` 并填入像素矩形，`KeyboardGridUI`
  会优先用它而不是自动网格布局。把这个资产和你的键盘美术图分别拖进场景里 `Systems` 物体上
  `KeyboardGridUI` 组件的 `Layout` / `Hand Drawn Keyboard Background` 字段即可。不指定的话会
  用内置的默认 QWERTY 布局和纯色格子，保证没有美术资源也能跑起来。
- **新的地图交互**：实现 `IInteractable`（参考 `Lever`/`DialogueTrigger`），挂上一个
  `Collider2D`（建议 `isTrigger`），`PlayerInteractor` 会自动识别。
- **新的对话/文字**：Assets ▸ Create ▸ Platformer ▸ Dialogue ▸ Dialogue Data 建一份数据，
  挂到任意带 `DialogueTrigger` 的物体上。

## Demo 场景里的机关（PlatformerDemo.unity）

从出生点往右依次是：告示牌（对话）→ 红色警示区（进入后 `D` 键被封锁，逼玩家打开 `Tab`
面板把“向右移动”挪到别的键上才能继续前进）→ 可跳上去的平台，上面有拉杆 → 拉杆控制右侧
的门。这些都只是用来验证各系统串联是否工作的最小示例，替换成你自己的关卡设计即可。

## 已知的手动步骤

场景中的可视元素（角色、地面、平台、拉杆、门、告示牌、警示区）目前用的是 Unity 内置占位
精灵 + 纯色，没有依赖任何自定义美术资源，方便直接在没有美术的情况下跑起来看效果。用 Unity
编辑器打开工程后，建议：
1. 确认 Player/Ground 等物体的 `SpriteRenderer` 使用的材质在你的 2D 渲染管线设置下能正常显示
   （不同 URP 2D 工程的默认精灵材质/光照设置可能不同）。
2. 把 `Systems` 物体上 `KeyboardGridUI` 的 `Hand Drawn Keyboard Background` 换成你手绘的键盘图，
   并按需要为各按键格子设置 `overrideRect` 精确对位。
3. 如需持久化玩家的按键分配（跨局保存），可以在 `KeyBindingManager` 之上加一层存读档，当前
   版本刻意不包含这部分，保持数据源单一、职责清晰。
