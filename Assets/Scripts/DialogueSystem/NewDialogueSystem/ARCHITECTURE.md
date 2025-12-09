# 新对话系统架构文档

## 系统架构总览

新对话系统采用模块化设计，支持多UI、多角色、多会话同时运行。

```
┌─────────────────────────────────────────────────────────┐
│              DialogueSystemManager (单例)                │
│  - 管理所有对话会话                                       │
│  - 管理UI实例                                             │
│  - 管理对话数据库                                         │
│  - 管理对话历史                                           │
└───────────────────┬─────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────▼────────┐    ┌─────────▼────────┐
│ DialogueSession│    │ DialogueDatabase │
│  - 节点流程控制 │    │  - 节点数据      │
│  - UI管理      │    │  - 角色数据      │
│  - 事件触发    │    │  - 数据验证      │
└───────┬────────┘    └──────────────────┘
        │
        │
┌───────▼────────────────────────────────────────┐
│           IDialogueUI (接口)                    │
│  ┌──────────────┬──────────────┬──────────────┐│
│  │ StoryDialogue│ BubbleDialogue│ChoiceDialogue││
│  │     UI       │      UI      │      UI      ││
│  └──────────────┴──────────────┴──────────────┘│
└─────────────────────────────────────────────────┘
```

## 核心组件详解

### 1. DialogueSystemManager

**职责**：
- 单例管理器，全局唯一
- 管理所有活动的对话会话
- 管理UI实例池
- 加载和管理对话数据库
- 管理对话历史记录

**关键方法**：
```csharp
DialogueSession StartDialogue(string startNodeID, string sessionID = null)
void EndDialogue(string sessionID)
DialogueSession GetSession(string sessionID)
IDialogueUI GetOrCreateUI(string uiInstanceID, DialogueDisplayMode displayMode)
```

### 2. DialogueSession

**职责**：
- 管理一次完整的对话流程
- 节点跳转和流程控制
- 条件判断和分支处理
- 事件触发

**状态流程**：
```
Start → GotoNode → ProcessNode → (等待用户操作/自动前进) → NextNode → ... → End
```

**支持的节点类型**：
- Text: 文本对话
- Choice: 选择分支
- Image: 图片插入
- Event: 事件触发
- Jump: 节点跳转
- End: 对话结束

### 3. DialogueDatabase

**职责**：
- 存储所有对话节点
- 存储所有角色数据
- 提供快速查找功能（字典索引）
- 数据验证

**数据结构**：
```csharp
List<DialogueNode> nodes          // 所有对话节点
List<CharacterData> characters    // 所有角色数据
Dictionary<string, DialogueNode> nodeDict      // 节点查找字典
Dictionary<string, CharacterData> characterDict // 角色查找字典
```

### 4. DialogueNode

**核心字段**：
- `nodeID`: 节点唯一标识
- `nodeType`: 节点类型
- `displayMode`: 显示模式（Story/Bubble等）
- `text`: 对话文本
- `characterID`: 角色ID
- `nextNodeID`: 下一个节点ID
- `choices`: 选择项列表
- `conditionalBranches`: 条件分支

### 5. IDialogueUI接口

**所有UI必须实现**：
```csharp
bool IsShowing { get; }
DialogueDisplayMode DisplayMode { get; }
System.Action OnDialogueCompleted { get; set; }
void ShowDialogue(DialogueNode node, CharacterData character);
void HideDialogue();
```

## 数据流程

### 启动对话流程

```
1. 调用 DialogueSystemManager.Instance.StartDialogue("start")
   ↓
2. 创建 DialogueSession
   ↓
3. 从数据库加载起始节点
   ↓
4. ProcessNode → 根据节点类型处理
   ↓
5. 获取/创建对应的UI实例
   ↓
6. UI显示对话内容
   ↓
7. 等待用户操作或自动前进
   ↓
8. 触发OnDialogueCompleted
   ↓
9. 跳转到下一个节点（重复4-8）或结束
```

### 多UI显示流程

```
同时启动两个对话：
├─ Session1 → UI Instance "ui_pet_001" (Bubble模式)
└─ Session2 → UI Instance "ui_pet_002" (Bubble模式)

两个UI可以同时显示，互不干扰
```

## UI系统架构

### UI层次结构

```
DialogueUIContainer (Canvas, SortingOrder: 200)
├── StoryDialogueUI_default (Canvas → Panel → Texts/Images)
├── BubbleDialogueUI_ui_pet_001 (Canvas → Panel → Text)
├── BubbleDialogueUI_ui_pet_002 (Canvas → Panel → Text)
└── ChoiceDialogueUI_choice (Canvas → Panel → Buttons)
```

### UI生命周期

```
实例化 → Awake → ShowDialogue → 显示内容 → HideDialogue → (可复用)
```

### UI基类功能

`DialogueUIBase` 提供：
- 父对象激活检查
- 打字动画
- 淡入淡出动画
- 自动前进

## 扩展点

### 1. 自定义UI类型

继承 `DialogueUIBase` 或实现 `IDialogueUI`：
```csharp
public class CustomDialogueUI : DialogueUIBase
{
    public override DialogueDisplayMode DisplayMode => DialogueDisplayMode.Custom;
    // 实现自定义显示逻辑
}
```

### 2. 条件系统扩展

在 `DialogueSession.EvaluateCondition()` 中实现：
- JSON条件表达式解析
- Lua脚本支持
- 变量系统

### 3. 事件系统扩展

在 `DialogueSession.ExecuteEvent()` 中实现：
- 事件注册机制
- 事件参数传递
- 事件链式调用

### 4. 脚本系统扩展

在 `DialogueSession.ExecuteScript()` 中实现：
- C#反射调用
- Lua脚本执行
- UnityEvent绑定

## 性能优化

1. **字典查找**：节点和角色使用字典索引，O(1)查找
2. **UI实例复用**：UI实例可以复用，减少实例化开销
3. **对象池**：可以扩展UI对象池功能
4. **延迟加载**：数据库可以按需加载

## 最佳实践

1. **节点ID命名**：使用有意义的命名，如 `story_chapter1_start`
2. **角色ID命名**：使用唯一标识，如 `pet_001`, `npc_001`
3. **UI实例ID**：多UI场景使用唯一ID，如 `ui_pet_001`
4. **数据库组织**：使用groups组织大型对话数据库
5. **错误处理**：总是检查节点是否存在，提供回退逻辑

## 总结

新对话系统设计：
- ✅ **模块化**：各组件职责清晰
- ✅ **可扩展**：易于添加新功能
- ✅ **高性能**：字典查找、实例复用
- ✅ **易用性**：简单的API、清晰的文档
- ✅ **灵活性**：支持多种对话模式、多UI显示

这是一个生产级别的对话系统架构！🎉

