# 从旧对话系统迁移到新对话系统指南

## 已修复的文件

### ✅ StoryModeManager.cs
- 已更新为使用 `NewDialogueSystem` 命名空间
- 已更新为使用 `DialogueSystemManager` 替代 `GalDialogueManager`

## 需要更新的文件（使用反射，暂时不会报错）

以下文件使用了反射访问旧对话系统，暂时不会导致编译错误，但应该逐步更新：

### 1. GameManager.cs
**位置**: `Assets/Scripts/GameManager.cs`

**需要更新的方法**:
- `InitializeDialogueSystem()` - 更新为新对话系统初始化
- `ConfigureDialogueSystemUI()` - 更新为新对话系统UI配置
- `SubscribeToDialogueEvents()` - 更新事件订阅
- `UnsubscribeFromDialogueEvents()` - 更新事件取消订阅
- `LoadDialogueDatabaseViaReflection()` - 更新为新对话系统加载

**迁移步骤**:
```csharp
// 旧代码（反射）
var galDialogueManagerType = System.Type.GetType("GalDialogueSystem.GalDialogueManager");

// 新代码（直接引用）
using NewDialogueSystem;
DialogueSystemManager.Instance...
```

### 2. PetController.cs
**位置**: `Assets/Scripts/Pet/PetController.cs`

**需要更新的方法**:
- `SubscribeToDialogueEvents()` - 更新事件订阅
- `UnsubscribeFromDialogueEvents()` - 更新事件取消订阅
- `IsDialogueActiveViaReflection()` - 更新为新对话系统
- `StartDialogueViaReflection()` - 更新为新对话系统启动对话
- `ShouldForceIdle()` - 更新对话检查逻辑

**迁移步骤**:
```csharp
// 旧代码（反射）
var galDialogueManagerType = System.Type.GetType("GalDialogueSystem.GalDialogueManager");
galDialogueManagerType.GetMethod("StartDialogue", ...).Invoke(...);

// 新代码（直接引用）
using NewDialogueSystem;
DialogueSystemManager.Instance.StartDialogue(nodeID);
```

### 3. IdleState.cs
**位置**: `Assets/Scripts/Pet/PetState/IdleState.cs`

**需要更新的方法**:
- `TryStartIdleChatter()` - 更新对话检查逻辑

**迁移步骤**:
```csharp
// 旧代码（反射）
var galDialogueManagerType = System.Type.GetType("GalDialogueSystem.GalDialogueManager");

// 新代码（直接引用）
using NewDialogueSystem;
// 检查是否有活跃的对话会话
// DialogueSystemManager.Instance.GetActiveSessionCount() > 0
```

## 快速迁移步骤

### 步骤1：添加命名空间引用

在每个需要更新的文件顶部添加：
```csharp
using NewDialogueSystem;
```

### 步骤2：替换管理器引用

将所有的 `GalDialogueManager` 替换为 `DialogueSystemManager`。

### 步骤3：更新方法调用

#### 启动对话
```csharp
// 旧
GalDialogueManager.Instance.StartDialogue(nodeID);

// 新
DialogueSystemManager.Instance.StartDialogue(nodeID);
```

#### 检查对话是否激活
```csharp
// 旧
bool isActive = GalDialogueManager.Instance.isDialogueActive;

// 新
// 需要检查是否有活跃的会话
bool isActive = DialogueSystemManager.Instance.GetActiveSessionCount() > 0;
// 或者保存会话ID并检查
// DialogueSession session = DialogueSystemManager.Instance.GetSession(sessionID);
// bool isActive = session != null && session.isActive;
```

#### 结束对话
```csharp
// 旧
GalDialogueManager.Instance.EndDialogue();

// 新
DialogueSystemManager.Instance.EndDialogue(sessionID);
```

### 步骤4：更新事件订阅

#### 对话结束事件
```csharp
// 旧
GalDialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;

// 新
DialogueSystemManager.Instance.OnDialogueSessionEnded += OnDialogueSessionEnded;
```

## 新对话系统的主要差异

1. **会话管理**: 新系统使用会话ID管理多个对话，而不是单一的全局对话状态
2. **UI管理**: UI实例通过UI实例ID管理，支持多UI同时显示
3. **事件系统**: 事件名称略有不同，如 `OnDialogueEnded` → `OnDialogueSessionEnded`

## 注意事项

1. **会话ID**: 新系统需要管理会话ID，如果需要在其他地方结束对话，需要保存会话ID
2. **多会话支持**: 新系统支持多个对话同时进行，可能需要调整对话检查逻辑
3. **向后兼容**: 如果需要兼容旧代码，可以创建适配器层

## 完整迁移示例

### 迁移前（使用反射）
```csharp
private void StartDialogueViaReflection(string nodeID)
{
    var galDialogueManagerType = System.Type.GetType("GalDialogueSystem.GalDialogueManager");
    if (galDialogueManagerType == null)
        return;
    
    var instanceProperty = galDialogueManagerType.GetProperty("Instance", ...);
    var instance = instanceProperty.GetValue(null);
    var startMethod = galDialogueManagerType.GetMethod("StartDialogue", ...);
    startMethod.Invoke(instance, new object[] { nodeID });
}
```

### 迁移后（直接引用）
```csharp
using NewDialogueSystem;

private string currentSessionID;

private void StartDialogue(string nodeID)
{
    if (DialogueSystemManager.Instance == null)
        return;
    
    currentSessionID = DialogueSystemManager.Instance.StartDialogue(nodeID)?.sessionID;
}
```

## 测试清单

迁移完成后，请测试：
- [ ] 对话可以正常启动
- [ ] 对话可以正常结束
- [ ] 对话历史记录功能正常
- [ ] 多个对话可以同时进行（如果使用）
- [ ] 事件订阅和取消订阅正常
- [ ] UI正确显示和隐藏

## 获取帮助

如果迁移过程中遇到问题，请参考：
- `README.md` - 完整系统文档
- `ARCHITECTURE.md` - 系统架构文档
- `QUICK_START.md` - 快速开始指南

