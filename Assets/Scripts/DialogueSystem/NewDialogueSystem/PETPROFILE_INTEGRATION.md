# PetProfileSO集成指南

本文档说明如何在PetProfileSO中配置新对话系统。

## 一、更新PetProfileSO

### 添加新对话系统配置字段

在 `PetProfileSO.cs` 中添加以下字段：

```csharp
[Header("新对话系统配置 (NewDialogueSystem)")]
[Tooltip("对话数据库JSON文件路径（相对于Resources文件夹，如：Dialogue/database）")]
public string newDialogueDatabasePath;

[Tooltip("剧情对话UI预制体")]
public GameObject newStoryDialogueUIPrefab;

[Tooltip("气泡对话UI预制体")]
public GameObject newBubbleDialogueUIPrefab;

[Tooltip("选择对话UI预制体")]
public GameObject newChoiceDialogueUIPrefab;

[Tooltip("历史记录UI预制体")]
public GameObject newHistoryDialogueUIPrefab;

[Tooltip("初始对话节点ID（进入剧情模式时触发）")]
public string newStartDialogueNodeID;

[Tooltip("点击对话节点ID（点击桌宠时触发）")]
public string newTouchDialogueNodeID;

[Tooltip("闲置对话节点ID列表")]
public List<string> newIdleDialogueNodeIDs = new List<string>();
```

## 二、在PetController中集成新对话系统

### 修改PetController启动对话的方法

```csharp
using NewDialogueSystem;

public class PetController : MonoBehaviour
{
    // ... 其他代码 ...

    /// <summary>
    /// 启动对话（使用新对话系统）
    /// </summary>
    public void StartNewDialogue(string nodeID)
    {
        if (DialogueSystemManager.Instance == null)
        {
            Debug.LogError("[PetController] DialogueSystemManager未初始化！");
            return;
        }

        // 启动对话会话
        string sessionID = $"pet_{Profile.petID}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        DialogueSystemManager.Instance.StartDialogue(nodeID, sessionID);
    }

    /// <summary>
    /// 触发点击对话
    /// </summary>
    public void TriggerTouchDialogue()
    {
        if (Profile != null && !string.IsNullOrEmpty(Profile.newTouchDialogueNodeID))
        {
            StartNewDialogue(Profile.newTouchDialogueNodeID);
        }
    }

    /// <summary>
    /// 触发闲置对话
    /// </summary>
    public void TriggerIdleDialogue()
    {
        if (Profile != null && Profile.newIdleDialogueNodeIDs != null && Profile.newIdleDialogueNodeIDs.Count > 0)
        {
            // 随机选择一个闲置对话节点
            int randomIndex = Random.Range(0, Profile.newIdleDialogueNodeIDs.Count);
            string nodeID = Profile.newIdleDialogueNodeIDs[randomIndex];
            StartNewDialogue(nodeID);
        }
    }
}
```

## 三、在GameManager中初始化新对话系统

### 配置DialogueSystemManager

```csharp
using NewDialogueSystem;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        // ... 其他初始化代码 ...

        // 初始化新对话系统
        InitializeNewDialogueSystem();
    }

    void InitializeNewDialogueSystem()
    {
        // 查找或创建DialogueSystemManager
        DialogueSystemManager manager = FindObjectOfType<DialogueSystemManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("DialogueSystemManager");
            manager = managerObj.AddComponent<DialogueSystemManager>();
        }

        // 如果场景中已有DialogueSystemManager，配置它
        // 这里可以根据需要配置默认UI预制体等
    }

    void Start()
    {
        // 配置桌宠的对话系统
        ConfigurePetDialogueSystem();
    }

    void ConfigurePetDialogueSystem()
    {
        // 从PetManager获取当前宠物配置
        PetProfileSO petProfile = PetManager.Instance?.GetCurrentPetProfile();
        if (petProfile == null)
            return;

        DialogueSystemManager manager = DialogueSystemManager.Instance;
        if (manager == null)
            return;

        // 配置数据库路径
        if (!string.IsNullOrEmpty(petProfile.newDialogueDatabasePath))
        {
            manager.databaseJsonPath = petProfile.newDialogueDatabasePath;
            manager.LoadDialogueDatabase();
        }

        // 配置默认UI预制体
        if (petProfile.newStoryDialogueUIPrefab != null)
        {
            manager.defaultStoryDialogueUIPrefab = petProfile.newStoryDialogueUIPrefab;
        }

        if (petProfile.newBubbleDialogueUIPrefab != null)
        {
            manager.defaultBubbleDialogueUIPrefab = petProfile.newBubbleDialogueUIPrefab;
        }

        if (petProfile.newChoiceDialogueUIPrefab != null)
        {
            manager.defaultChoiceDialogueUIPrefab = petProfile.newChoiceDialogueUIPrefab;
        }

        if (petProfile.newHistoryDialogueUIPrefab != null)
        {
            manager.defaultHistoryDialogueUIPrefab = petProfile.newHistoryDialogueUIPrefab;
        }
    }
}
```

## 四、在StoryModeManager中集成

### 进入剧情模式时启动对话

```csharp
using NewDialogueSystem;

public class StoryModeManager : MonoBehaviour
{
    public void EnterStoryMode(PetProfileSO petProfile)
    {
        // ... 其他进入剧情模式的代码 ...

        // 启动初始对话
        if (petProfile != null && !string.IsNullOrEmpty(petProfile.newStartDialogueNodeID))
        {
            DialogueSystemManager.Instance?.StartDialogue(petProfile.newStartDialogueNodeID);
        }
    }
}
```

## 五、完整配置示例

### PetProfileSO配置示例

在Unity Inspector中配置PetProfileSO：

```
New Dialogue Database Path: Dialogue/pet_001_database
New Story Dialogue UI Prefab: [StoryDialogueUI预制体]
New Bubble Dialogue UI Prefab: [BubbleDialogueUI预制体]
New Choice Dialogue UI Prefab: [ChoiceDialogueUI预制体]
New History Dialogue UI Prefab: [HistoryDialogueUI预制体]
New Start Dialogue Node ID: story_start
New Touch Dialogue Node ID: touch_hello
New Idle Dialogue Node IDs:
  - idle_001
  - idle_002
  - idle_003
```

## 六、数据流程

1. **游戏启动**：
   - GameManager初始化DialogueSystemManager
   - 从PetProfileSO加载对话数据库
   - 配置UI预制体

2. **桌宠交互**：
   - 点击桌宠 → 触发 `newTouchDialogueNodeID` 对话
   - 闲置时 → 随机触发 `newIdleDialogueNodeIDs` 中的对话

3. **剧情模式**：
   - 进入剧情模式 → 触发 `newStartDialogueNodeID` 对话

## 七、注意事项

1. **数据库路径**：确保JSON文件在Resources文件夹下
2. **UI预制体**：确保预制体已正确配置（参考UI_SETUP_GUIDE.md）
3. **节点ID**：确保JSON中的节点ID与配置的节点ID匹配
4. **角色ID**：确保JSON中的角色ID与配置的角色ID匹配

## 八、测试建议

1. 创建测试对话数据库JSON
2. 创建简单的气泡对话UI预制体
3. 在PetProfileSO中配置
4. 运行游戏测试点击桌宠是否触发对话

## 完成！

现在你已经在PetProfileSO中集成了新对话系统！🎉

