# 新对话系统 (NewDialogueSystem) - 完整文档

## 系统概述

新对话系统是一个功能完整、可扩展的对话系统，专为Unity 2022.3设计，支持Galgame风格的对话功能。

### 核心特性

1. ✅ **多重选择对话** - 支持分支选择和条件判断
2. ✅ **图片插入** - 支持角色立绘、背景图片、插入图片
3. ✅ **多角色多UI对话** - 可以同时显示多个角色的对话UI
4. ✅ **多种对话模式** - 剧情对话、气泡对话、通知模式等
5. ✅ **历史记录UI** - 完整的对话历史查看功能
6. ✅ **JSON/XML配置** - 灵活的配置文件格式
7. ✅ **PetProfileSO集成** - 可在PetProfile中配置
8. ✅ **Unity 2022.3 UI标准** - 完全支持Unity标准UI创建方式

## 系统架构

### 目录结构

```
NewDialogueSystem/
├── Core/                    # 核心管理器
│   ├── DialogueSystemManager.cs    # 主管理器（单例）
│   └── DialogueSession.cs          # 对话会话
├── Data/                    # 数据模型
│   ├── DialogueNode.cs             # 对话节点
│   ├── DialogueDatabase.cs         # 对话数据库
│   └── CharacterData.cs            # 角色数据
├── UI/                      # UI组件
│   ├── IDialogueUI.cs              # UI接口
│   └── DialogueUIBase.cs           # UI基类
└── Loader/                  # 数据加载器
    └── DialogueDatabaseLoader.cs   # JSON/XML加载器
```

### 核心组件

#### 1. DialogueSystemManager（主管理器）

单例模式，负责：
- 管理所有对话会话
- 管理UI实例
- 管理对话数据库
- 管理对话历史记录

#### 2. DialogueSession（对话会话）

管理一次完整的对话流程：
- 节点跳转和流程控制
- UI实例管理
- 事件触发
- 条件判断

#### 3. DialogueDatabase（对话数据库）

存储所有对话数据：
- 对话节点列表
- 角色数据列表
- 对话组（可选）

#### 4. DialogueNode（对话节点）

单个对话节点的数据：
- 节点类型（Text, Choice, Image, Event, Jump, End）
- 对话内容
- 角色信息
- 图片资源
- 音频资源
- 分支和流程控制

## 快速开始

### 第一步：创建对话数据库JSON文件

在 `Resources/Dialogue/` 目录下创建 `database.json` 文件：

```json
{
    "databaseID": "main_database",
    "databaseName": "主对话数据库",
    "version": "1.0",
    "nodes": [
        {
            "nodeID": "start",
            "nodeType": 0,
            "displayMode": 0,
            "characterID": "pet_001",
            "text": "你好！欢迎使用新对话系统！",
            "textSpeed": 30.0,
            "autoAdvanceTime": 0.0,
            "nextNodeID": "choice_001"
        },
        {
            "nodeID": "choice_001",
            "nodeType": 1,
            "displayMode": 0,
            "text": "请选择：",
            "choices": [
                {
                    "text": "选项1",
                    "nextNodeID": "response_001"
                },
                {
                    "text": "选项2",
                    "nextNodeID": "response_002"
                }
            ]
        },
        {
            "nodeID": "response_001",
            "nodeType": 0,
            "displayMode": 0,
            "characterID": "pet_001",
            "text": "你选择了选项1！",
            "nextNodeID": "end"
        },
        {
            "nodeID": "response_002",
            "nodeType": 0,
            "displayMode": 0,
            "characterID": "pet_001",
            "text": "你选择了选项2！",
            "nextNodeID": "end"
        },
        {
            "nodeID": "end",
            "nodeType": 5
        }
    ],
    "characters": [
        {
            "characterID": "pet_001",
            "characterName": "小宠物",
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
            "voicePitch": 1.0,
            "defaultTextSpeed": 30.0
        }
    ],
    "groups": []
}
```

### 第二步：在场景中设置DialogueSystemManager

1. 在场景中创建空GameObject，命名为 `DialogueSystemManager`
2. 添加 `DialogueSystemManager` 组件
3. 配置组件：
   - **Database Json Path**: `Dialogue/database` (相对于Resources文件夹)
   - **Default Story Dialogue UI Prefab**: 指定剧情对话UI预制体
   - **Default Bubble Dialogue UI Prefab**: 指定气泡对话UI预制体
   - **Default Choice Dialogue UI Prefab**: 指定选择对话UI预制体
   - **Default History Dialogue UI Prefab**: 指定历史记录UI预制体

### 第三步：创建UI预制体

#### 创建剧情对话UI预制体

按照Unity 2022.3标准方式创建：

1. **创建根GameObject**：
   - 在Project窗口右键 → Create Empty
   - 命名为 `StoryDialogueUI`

2. **添加组件**：
   - 添加 `StoryDialogueUI` 脚本（继承自DialogueUIBase）
   - 添加 `Canvas Group` 组件

3. **创建UI结构**（使用Unity UI菜单）：
   ```
   StoryDialogueUI
   ├── Canvas (Unity自动创建)
   │   ├── Canvas
   │   ├── CanvasScaler
   │   └── GraphicRaycaster
   │   └── Panel (UI → Panel)
   │       ├── CharacterNameText (UI → TextMeshPro - Text (UI))
   │       ├── DialogueText (UI → TextMeshPro - Text (UI))
   │       ├── CharacterPortrait (UI → Image)
   │       └── BackgroundImage (UI → Image)
   ```

4. **配置脚本引用**：
   - 将Panel拖到 `Dialogue Panel`
   - 将CharacterNameText拖到 `Character Name Text`
   - 将DialogueText拖到 `Dialogue Text`
   - 将CharacterPortrait拖到 `Character Portrait`
   - 将BackgroundImage拖到 `Background Image`

5. **保存为预制体**

详细步骤请参考 `UI_SETUP_GUIDE.md`

### 第四步：在代码中启动对话

```csharp
using NewDialogueSystem;

public class ExampleDialogueStarter : MonoBehaviour
{
    void Start()
    {
        // 启动对话
        DialogueSystemManager.Instance.StartDialogue("start");
    }
}
```

## UI预制体制作指南

### Unity 2022.3 UI创建方式

Unity 2022.3使用UI菜单时会自动创建Canvas，这是标准的创建方式，系统完全支持。

### 创建气泡对话UI预制体

1. 创建根GameObject：`BubbleDialogueUI`
2. 添加组件：
   - `BubbleDialogueUI` 脚本
   - `Canvas Group`
3. 使用UI菜单创建：
   - 右键 → UI → Panel（Unity自动创建Canvas）
   - Panel下创建Text：UI → TextMeshPro - Text (UI)
4. 配置脚本引用

详细步骤请参考 `UI_SETUP_GUIDE.md`

## 高级功能

### 多UI同时显示

可以通过指定不同的 `uiInstanceID` 来同时显示多个对话UI：

```json
{
    "nodeID": "multi_dialogue",
    "uiInstanceID": "ui_pet_001",
    "displayMode": 1,
    "text": "宠物1的对话"
}
```

另一个节点可以同时显示：

```json
{
    "nodeID": "multi_dialogue_2",
    "uiInstanceID": "ui_pet_002",
    "displayMode": 1,
    "text": "宠物2的对话"
}
```

### 条件分支

节点支持条件分支：

```json
{
    "nodeID": "conditional_node",
    "conditionalBranches": [
        {
            "condition": "{\"variable\":\"affection\",\"operator\":\">\",\"value\":50}",
            "nextNodeID": "high_affection",
            "priority": 1
        },
        {
            "condition": "true",
            "nextNodeID": "normal",
            "priority": 0
        }
    ]
}
```

### 图片插入

节点支持插入多张图片：

```json
{
    "nodeID": "image_node",
    "portraitSpritePath": "Characters/pet_portrait",
    "backgroundImagePath": "Backgrounds/scene_001",
    "insertImagePaths": [
        "Images/insert_001",
        "Images/insert_002"
    ]
}
```

## PetProfileSO集成

在PetProfileSO中添加对话系统配置：

```csharp
[Header("新对话系统配置")]
public string dialogueDatabasePath = "Dialogue/database";
public GameObject storyDialogueUIPrefab;
public GameObject bubbleDialogueUIPrefab;
public string startDialogueNodeID = "start";
public string touchDialogueNodeID = "touch_dialogue";
```

## 常见问题

### Q: UI不显示怎么办？

A: 检查以下几点：
1. DialogueSystemManager是否正确初始化
2. UI预制体的Canvas是否正确配置
3. UI预制体的所有父对象是否激活
4. 查看Console中的错误信息

### Q: 如何调试对话流程？

A: 在Console中会输出详细的日志信息，包括：
- 节点跳转信息
- UI创建信息
- 错误和警告信息

### Q: 如何自定义UI样式？

A: 继承 `DialogueUIBase` 或实现 `IDialogueUI` 接口，创建自定义UI类。

## API参考

### DialogueSystemManager

- `StartDialogue(string startNodeID, string sessionID = null)` - 开始对话
- `EndDialogue(string sessionID)` - 结束对话
- `GetSession(string sessionID)` - 获取对话会话
- `GetOrCreateUI(string uiInstanceID, DialogueDisplayMode displayMode)` - 获取或创建UI实例
- `AddHistoryEntry(DialogueHistoryEntry entry)` - 添加历史记录

### DialogueSession

- `GotoNode(string nodeID)` - 跳转到指定节点
- `End()` - 结束会话

## 更新日志

### v1.0.0 (2024-01-XX)
- 初始版本
- 支持基础对话功能
- 支持多种对话模式
- 支持多UI显示
- 支持JSON配置

## 许可证

本项目使用 MIT 许可证。

