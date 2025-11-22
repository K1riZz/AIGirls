# GalDialogue System - GalGame风格对话系统

## 概述

GalDialogue System 是一个专为桌面宠物游戏设计的 GalGame 风格对话系统，参考了传统 GalGame 的对话框架。系统支持多重选择对话、图片插入、多角色多UI对话、不同对话模式（剧情对话、气泡对话等）、历史记录查看以及 JSON/XML 配置。

## 核心功能

1. **多重选择对话** - 支持分支选择，根据玩家选择跳转到不同对话节点
2. **图片插入** - 支持角色立绘、背景图片、插入图片等
3. **多角色多UI对话** - 支持同时显示多个角色的对话UI
4. **不同对话模式** - 支持剧情对话（全屏AVG模式）、气泡对话（桌宠模式）、通知模式等
5. **历史记录** - 支持查看对话历史记录，可滚动查看
6. **配置系统** - 支持 JSON 和 XML 格式的对话数据配置
7. **PetProfileSO 集成** - 可在 PetProfileSO 中配置对话数据库和UI预制体

## 系统架构

### 1. 核心数据模型

#### DialogueNode（对话节点）
- `nodeID`: 节点唯一标识
- `nodeType`: 节点类型（Text/Choice/Image/Event/End）
- `dialogueMode`: 对话模式（Story/Bubble/Notification/Custom）
- `characterID`: 角色ID
- `text`: 对话文本内容
- `characterName`: 角色名称
- `portraitSpritePath`: 角色头像/立绘路径
- `backgroundImagePath`: 背景图片路径
- `insertImagePath`: 插入图片路径
- `choices`: 选择项列表
- `nextNodeID`: 下一个节点ID
- `eventName`: 事件名称
- `eventData`: 事件数据（JSON格式）
- `condition`: 显示条件
- `textSpeed`: 对话速度（字符/秒）
- `autoAdvanceTime`: 自动前进时间（秒）
- `soundEffectPath`: 音效路径
- `backgroundMusicPath`: 背景音乐路径

#### DialogueChoice（对话选择项）
- `text`: 选择项文本
- `nextNodeID`: 选择后跳转到的节点ID
- `condition`: 显示条件
- `effect`: 选择后的特殊效果

#### CharacterData（角色数据）
- `characterID`: 角色ID
- `characterName`: 角色名称
- `defaultPortraitPath`: 角色默认头像/立绘路径
- `nameColor`: 角色名称颜色
- `textColor`: 角色对话文本颜色
- `voiceVolume`: 角色语音音量
- `voicePitch`: 角色默认语音音调

#### DialogueDatabase（对话数据库）
- `databaseID`: 数据库ID
- `databaseName`: 数据库名称
- `characters`: 角色列表
- `nodes`: 对话节点字典
- `entryNodeIDs`: 对话起点节点ID列表

### 2. 核心管理器

#### GalDialogueManager（对话管理器）
- 单例模式，负责管理对话流程
- 加载对话数据库
- 处理对话节点跳转
- 管理对话历史记录
- 触发对话事件

主要方法：
- `LoadDialogueDatabase(string filePath)`: 加载对话数据库
- `StartDialogue(string entryNodeID)`: 开始对话
- `CompleteCurrentNode()`: 完成当前节点
- `SelectChoice(DialogueChoice choice)`: 选择选择项
- `EndDialogue()`: 结束对话
- `JumpToNode(string nodeID)`: 跳转到指定节点

#### DialogueUIManager（UI管理器）
- 单例模式，负责管理所有对话UI
- 根据对话模式切换不同的UI
- 处理UI显示和隐藏
- 管理选择UI和历史记录UI

### 3. UI系统

#### IDialogueUI（对话UI接口）
所有对话UI都必须实现此接口：
- `ShowDialogue(DialogueNode node, CharacterData character)`: 显示对话
- `HideDialogue()`: 隐藏对话
- `IsShowing`: 是否正在显示
- `DialogueMode`: 对话模式
- `OnDialogueCompleted`: 对话完成回调

#### StoryDialogueUI（剧情对话UI）
- 全屏AVG模式的对话UI
- 显示角色名称、对话文本、角色立绘、背景图片
- 支持打字动画
- 支持自动前进
- 支持点击继续

#### BubbleDialogueUI（气泡对话UI）
- 桌宠模式的气泡对话UI
- 显示对话文本
- 支持打字动画
- 支持自动隐藏
- 支持点击继续

#### ChoiceDialogueUI（选择对话UI）
- 多重选择的对话UI
- 动态生成选择按钮
- 支持条件显示
- 处理选择事件

#### HistoryUI（历史记录UI）
- 显示对话历史记录
- 支持滚动查看
- 支持清空历史
- 按H键显示/隐藏

### 4. 数据加载器

#### DialogueDataLoader（对话数据加载器）
- 支持从JSON文件加载对话数据
- 支持从XML文件加载对话数据（待实现）
- 支持保存对话数据为JSON格式

主要方法：
- `LoadFromJSON(string filePath)`: 从JSON文件加载
- `LoadFromXML(string filePath)`: 从XML文件加载（待实现）
- `SaveToJSON(DialogueDatabase database, string filePath)`: 保存为JSON文件

## JSON配置文件格式

### 对话数据库JSON格式示例

```json
{
  "databaseID": "main_database",
  "databaseName": "主对话数据库",
  "entryNodeIDs": ["start_01", "start_02"],
  "characters": [
    {
      "characterID": "pet_001",
      "characterName": "小猫",
      "defaultPortraitPath": "Characters/pet_portrait",
      "nameColor": {
        "r": 1.0,
        "g": 1.0,
        "b": 1.0,
        "a": 1.0
      },
      "textColor": {
        "r": 1.0,
        "g": 1.0,
        "b": 1.0,
        "a": 1.0
      },
      "voiceVolume": 1.0,
      "voicePitch": 1.0
    }
  ],
  "nodes": [
    {
      "nodeID": "start_01",
      "nodeType": "Text",
      "dialogueMode": "Story",
      "characterID": "pet_001",
      "text": "你好，欢迎来到我的世界！",
      "characterName": "小猫",
      "portraitSpritePath": "Characters/pet_portrait",
      "backgroundImagePath": "Backgrounds/room_01",
      "nextNodeID": "start_02",
      "textSpeed": 30.0,
      "autoAdvanceTime": 0.0
    },
    {
      "nodeID": "start_02",
      "nodeType": "Choice",
      "dialogueMode": "Story",
      "characterID": "pet_001",
      "text": "你想做什么？",
      "characterName": "小猫",
      "choices": [
        {
          "text": "和你聊天",
          "nextNodeID": "chat_01",
          "condition": "",
          "effect": ""
        },
        {
          "text": "一起玩游戏",
          "nextNodeID": "game_01",
          "condition": "",
          "effect": ""
        },
        {
          "text": "再见",
          "nextNodeID": "end_01",
          "condition": "",
          "effect": ""
        }
      ],
      "textSpeed": 30.0,
      "autoAdvanceTime": 0.0
    },
    {
      "nodeID": "bubble_01",
      "nodeType": "Text",
      "dialogueMode": "Bubble",
      "characterID": "pet_001",
      "text": "喵~",
      "characterName": "小猫",
      "nextNodeID": "",
      "textSpeed": 50.0,
      "autoAdvanceTime": 3.0
    }
  ]
}
```

### 资源路径说明

所有资源路径都相对于 `Resources` 文件夹：
- 角色立绘：`Characters/pet_portrait` → `Resources/Characters/pet_portrait.png`
- 背景图片：`Backgrounds/room_01` → `Resources/Backgrounds/room_01.png`
- 插入图片：`Images/image_01` → `Resources/Images/image_01.png`
- 音效：`Sounds/click` → `Resources/Sounds/click.wav`
- 背景音乐：`Music/bgm_01` → `Resources/Music/bgm_01.mp3`

## UI架构说明

### 1. UI层级结构

```
DialogueUIContainer (GameObject)
├── StoryDialogueUI (StoryDialogueUI Component)
│   ├── DialoguePanel (GameObject)
│   │   ├── CharacterNameText (TextMeshProUGUI)
│   │   ├── DialogueText (TextMeshProUGUI)
│   │   ├── CharacterPortrait (Image)
│   │   ├── BackgroundImage (Image)
│   │   ├── InsertImage (Image)
│   │   ├── ContinueHint (GameObject)
│   │   └── AutoAdvanceSlider (Slider)
│   └── CanvasGroup (CanvasGroup)
│
├── BubbleDialogueUI (BubbleDialogueUI Component)
│   ├── BubblePanel (GameObject)
│   │   ├── BubbleText (TextMeshProUGUI)
│   │   └── BubbleBackground (Image)
│   └── CanvasGroup (CanvasGroup)
│
├── ChoiceDialogueUI (ChoiceDialogueUI Component)
│   ├── ChoicePanel (GameObject)
│   └── ChoiceButtonContainer (Transform)
│       └── ChoiceButton (Button) [动态生成]
│           └── ChoiceText (TextMeshProUGUI)
│
└── HistoryUI (HistoryUI Component)
    ├── HistoryPanel (GameObject)
    ├── ScrollRect (ScrollRect)
    │   └── HistoryContent (Transform)
    │       └── HistoryEntry (GameObject) [动态生成]
    │           ├── CharacterNameText (TextMeshProUGUI)
    │           ├── DialogueText (TextMeshProUGUI)
    │           └── TimeText (TextMeshProUGUI)
    ├── CloseButton (Button)
    └── ClearButton (Button)
```

### 2. Canvas设置

所有对话UI应该放在独立的Canvas上，建议：
- Canvas Render Mode: `Screen Space - Overlay`
- Canvas Sort Order: 200（确保在桌宠UI之上）
- Canvas Scaler: `Scale With Screen Size`
- Reference Resolution: `1920x1080`

### 3. UI预制体要求

#### StoryDialogueUI 预制体
- 必须包含 `StoryDialogueUI` 组件
- 必须实现 `IDialogueUI` 接口的所有方法
- 必须设置所有必需的UI组件引用
- 建议使用全屏布局（锚点拉伸到全屏）

#### BubbleDialogueUI 预制体
- 必须包含 `BubbleDialogueUI` 组件
- 必须实现 `IDialogueUI` 接口的所有方法
- 必须设置气泡面板和文本组件
- 建议使用相对定位（跟随桌宠位置）

#### ChoiceDialogueUI 预制体
- 必须包含 `ChoiceDialogueUI` 组件
- 必须设置选择按钮预制体
- 必须设置选择按钮容器（Vertical Layout Group 或 Grid Layout Group）

#### HistoryUI 预制体
- 必须包含 `HistoryUI` 组件
- 必须设置历史记录滚动视图
- 必须设置历史记录条目预制体
- 建议使用全屏或半屏布局

### 4. UI动画和效果

#### 打字动画
- 使用协程逐字符显示文本
- 可配置打字速度（字符/秒）
- 支持点击跳过打字动画

#### 淡入淡出动画
- 使用 `CanvasGroup.alpha` 实现淡入淡出
- 默认淡入淡出时间为 0.3 秒
- 可在UI组件中自定义淡入淡出时间

#### 自动前进
- 支持自动前进到下一个节点
- 可配置自动前进时间（秒）
- 显示自动前进进度条（可选）

## 使用流程

### 1. 初始化对话系统

在 `GameManager` 或场景初始化时：

```csharp
// 确保GalDialogueManager存在
if (GalDialogueManager.Instance == null)
{
    GameObject dialogueManagerObj = new GameObject("GalDialogueManager");
    dialogueManagerObj.AddComponent<GalDialogueManager>();
}

// 确保DialogueUIManager存在
if (DialogueUIManager.Instance == null)
{
    GameObject uiManagerObj = new GameObject("DialogueUIManager");
    uiManagerObj.AddComponent<DialogueUIManager>();
}
```

### 2. 加载对话数据库

在 `PetProfileSO` 中配置对话数据库路径，然后在 `GalDialogueManager.Start()` 中自动加载，或手动加载：

```csharp
// 从PetProfileSO加载
if (petProfile != null && !string.IsNullOrEmpty(petProfile.dialogueDatabasePath))
{
    GalDialogueManager.Instance.LoadDialogueDatabase(petProfile.dialogueDatabasePath);
}
```

### 3. 配置UI预制体

在 `DialogueUIManager` 或 `PetProfileSO` 中配置UI预制体：

```csharp
// 在PetProfileSO中配置
petProfile.storyDialogueUIPrefab = storyDialogueUIPrefab;
petProfile.bubbleDialogueUIPrefab = bubbleDialogueUIPrefab;
petProfile.choiceDialogueUIPrefab = choiceDialogueUIPrefab;
petProfile.historyUIPrefab = historyUIPrefab;
```

### 4. 开始对话

```csharp
// 开始对话
GalDialogueManager.Instance.StartDialogue("start_01");

// 或者在PetController中使用
if (GalDialogueManager.Instance != null && !string.IsNullOrEmpty(profile.startDialogueNodeID))
{
    GalDialogueManager.Instance.StartDialogue(profile.startDialogueNodeID);
}
```

### 5. 监听对话事件

```csharp
// 订阅对话事件
GalDialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
GalDialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
GalDialogueManager.Instance.OnDialogueNodeStarted += OnDialogueNodeStarted;
GalDialogueManager.Instance.OnChoiceSelected += OnChoiceSelected;
```

## PetProfileSO 配置说明

在 `PetProfileSO` 中可以配置以下新对话系统相关字段：

- `dialogueDatabasePath`: 对话数据库JSON文件路径（相对于Resources文件夹）
- `storyDialogueUIPrefab`: 剧情对话UI预制体
- `bubbleDialogueUIPrefab`: 气泡对话UI预制体
- `choiceDialogueUIPrefab`: 选择对话UI预制体
- `historyUIPrefab`: 历史记录UI预制体
- `startDialogueNodeID`: 初始对话节点ID
- `touchDialogueNodeID`: 点击对话节点ID
- `idleDialogueNodeIDs`: 闲置对话节点ID列表

## 扩展开发

### 添加新的对话模式

1. 创建新的UI类，实现 `IDialogueUI` 接口
2. 在 `DialogueUIManager` 中注册新的UI
3. 在 `DialogueMode` 枚举中添加新模式
4. 在 `DialogueNode` 中设置新的对话模式

### 添加条件系统

1. 实现条件检查逻辑（Lua表达式或JSON条件）
2. 在 `DialogueNode.CanDisplay()` 和 `DialogueChoice.CanDisplay()` 中调用条件检查
3. 支持变量系统（如好感度、物品数量等）

### 添加事件系统

1. 实现事件触发和处理逻辑
2. 在 `GalDialogueManager.TriggerEvent()` 中处理事件
3. 支持自定义事件类型（如改变变量、触发动画、播放音效等）

## 注意事项

1. **资源路径**：所有资源路径都相对于 `Resources` 文件夹，确保资源放在正确的位置
2. **节点ID**：确保节点ID唯一，避免跳转错误
3. **UI预制体**：确保UI预制体正确设置所有必需的组件引用
4. **Canvas层级**：确保对话UI的Canvas层级高于桌宠UI
5. **内存管理**：历史记录会持续增长，建议添加历史记录数量限制

## 故障排除

### 对话不显示
- 检查对话数据库是否正确加载
- 检查节点ID是否正确
- 检查UI预制体是否正确配置
- 检查Canvas层级设置

### 选择项不显示
- 检查选择项条件是否满足
- 检查选择按钮预制体是否正确设置
- 检查选择按钮容器是否正确配置

### 历史记录为空
- 检查历史记录是否在对话完成时正确添加
- 检查历史记录UI是否正确初始化

### 资源加载失败
- 检查资源路径是否正确
- 检查资源是否存在于Resources文件夹中
- 检查资源文件扩展名是否正确

## 未来计划

- [ ] 实现XML配置文件支持
- [ ] 实现条件系统（Lua表达式或JSON条件）
- [ ] 实现事件系统（变量、动画、音效等）
- [ ] 实现语音系统
- [ ] 实现存档系统
- [ ] 实现本地化支持
- [ ] 实现对话编辑器工具
