# GalDialogue System - 快速开始指南

## 第一步：创建对话数据库JSON文件

1. 在 `Assets/Resources/Dialogue/` 目录下创建你的对话数据库JSON文件（例如：`my_dialogue.json`）
2. 可以参考 `ExampleDialogue.json` 的格式
3. 确保JSON格式正确（可以使用在线JSON验证工具）

## 第二步：配置PetProfileSO

1. 选择你的 `PetProfileSO` 资源
2. 在 Inspector 中找到 "新对话系统配置（GalDialogue System）" 部分
3. 设置以下字段：
   - `Dialogue Database Path`: 对话数据库JSON文件路径（例如：`Dialogue/my_dialogue`，不需要扩展名和Resources前缀）
   - `Start Dialogue Node ID`: 初始对话节点ID（例如：`start_01`）
   - `Touch Dialogue Node ID`: 点击对话节点ID（例如：`bubble_hello`）
   - `Idle Dialogue Node IDs`: 闲置对话节点ID列表（例如：`bubble_happy`, `bubble_sleepy`）

## 第三步：创建UI预制体

### 1. 创建剧情对话UI（StoryDialogueUI）

1. 在场景中创建一个新的GameObject，命名为 `StoryDialogueUI`
2. 添加 `Canvas` 组件：
   - Render Mode: `Screen Space - Overlay`
   - Sort Order: 200
   - Canvas Scaler: `Scale With Screen Size`
   - Reference Resolution: `1920x1080`
3. 添加 `StoryDialogueUI` 组件
4. 创建UI结构：
   - 创建一个Panel作为对话面板
   - 在Panel中添加：
     - TextMeshProUGUI（角色名称）
     - TextMeshProUGUI（对话文本）
     - Image（角色立绘）
     - Image（背景图片）
     - Image（插入图片）
     - GameObject（继续提示）
     - Slider（自动前进进度条，可选）
5. 在 `StoryDialogueUI` 组件中设置所有UI组件引用
6. 将GameObject保存为Prefab：`Assets/Prefabs/UI/StoryDialogueUI.prefab`

### 2. 创建气泡对话UI（BubbleDialogueUI）

1. 在场景中创建一个新的GameObject，命名为 `BubbleDialogueUI`
2. 添加 `Canvas` 组件（同上）
3. 添加 `BubbleDialogueUI` 组件
4. 创建UI结构：
   - 创建一个Panel作为气泡面板
   - 在Panel中添加：
     - Image（气泡背景）
     - TextMeshProUGUI（气泡文本）
5. 在 `BubbleDialogueUI` 组件中设置所有UI组件引用
6. 将GameObject保存为Prefab：`Assets/Prefabs/UI/BubbleDialogueUI.prefab`

### 3. 创建选择对话UI（ChoiceDialogueUI）

1. 在场景中创建一个新的GameObject，命名为 `ChoiceDialogueUI`
2. 添加 `Canvas` 组件（同上）
3. 添加 `ChoiceDialogueUI` 组件
4. 创建UI结构：
   - 创建一个Panel作为选择面板
   - 在Panel中添加：
     - GameObject（选择按钮容器，添加 `Vertical Layout Group` 或 `Grid Layout Group`）
   - 创建一个Button作为选择按钮预制体：
     - Button
     - TextMeshProUGUI（选择文本）
5. 在 `ChoiceDialogueUI` 组件中设置：
   - 选择面板引用
   - 选择按钮预制体引用
   - 选择按钮容器引用
6. 将GameObject保存为Prefab：`Assets/Prefabs/UI/ChoiceDialogueUI.prefab`
7. 将选择按钮保存为Prefab：`Assets/Prefabs/UI/ChoiceButton.prefab`

### 4. 创建历史记录UI（HistoryUI）

1. 在场景中创建一个新的GameObject，命名为 `HistoryUI`
2. 添加 `Canvas` 组件（同上）
3. 添加 `HistoryUI` 组件
4. 创建UI结构：
   - 创建一个Panel作为历史记录面板（全屏或半屏）
   - 在Panel中添加：
     - ScrollRect（滚动视图）
     - GameObject（历史记录内容容器，作为ScrollRect的Content）
     - Button（关闭按钮）
     - Button（清空按钮）
   - 创建一个历史记录条目预制体：
     - TextMeshProUGUI（角色名称）
     - TextMeshProUGUI（对话文本）
     - TextMeshProUGUI（时间）
5. 在 `HistoryUI` 组件中设置：
   - 历史记录面板引用
   - 滚动视图引用
   - 历史记录内容容器引用
   - 历史记录条目预制体引用
   - 关闭按钮引用
   - 清空按钮引用
6. 将GameObject保存为Prefab：`Assets/Prefabs/UI/HistoryUI.prefab`
7. 将历史记录条目保存为Prefab：`Assets/Prefabs/UI/HistoryEntry.prefab`

## 第四步：在PetProfileSO中配置UI预制体

1. 选择你的 `PetProfileSO` 资源
2. 在 Inspector 中找到 "新对话系统配置（GalDialogue System）" 部分
3. 将创建的UI预制体拖拽到相应字段：
   - `Story Dialogue UI Prefab`: `StoryDialogueUI.prefab`
   - `Bubble Dialogue UI Prefab`: `BubbleDialogueUI.prefab`
   - `Choice Dialogue UI Prefab`: `ChoiceDialogueUI.prefab`
   - `History UI Prefab`: `HistoryUI.prefab`

## 第五步：初始化对话系统

在 `GameManager` 或场景初始化脚本中添加：

```csharp
using GalDialogueSystem;

void Start()
{
    // 确保GalDialogueManager存在
    if (GalDialogueManager.Instance == null)
    {
        GameObject dialogueManagerObj = new GameObject("GalDialogueManager");
        dialogueManagerObj.AddComponent<GalDialogueManager>();
        DontDestroyOnLoad(dialogueManagerObj);
    }

    // 确保DialogueUIManager存在
    if (DialogueUIManager.Instance == null)
    {
        GameObject uiManagerObj = new GameObject("DialogueUIManager");
        DialogueUIManager uiManager = uiManagerObj.AddComponent<DialogueUIManager>();
        DontDestroyOnLoad(uiManagerObj);
    }

    // 从PetProfileSO加载对话配置
    if (currentPetProfile != null)
    {
        // 加载对话数据库
        if (!string.IsNullOrEmpty(currentPetProfile.dialogueDatabasePath))
        {
            GalDialogueManager.Instance.LoadDialogueDatabase(currentPetProfile.dialogueDatabasePath);
        }

        // 配置UI预制体
        if (DialogueUIManager.Instance != null)
        {
            DialogueUIManager.Instance.storyDialogueUIPrefab = currentPetProfile.storyDialogueUIPrefab;
            DialogueUIManager.Instance.bubbleDialogueUIPrefab = currentPetProfile.bubbleDialogueUIPrefab;
            DialogueUIManager.Instance.choiceDialogueUIPrefab = currentPetProfile.choiceDialogueUIPrefab;
            DialogueUIManager.Instance.historyUIPrefab = currentPetProfile.historyUIPrefab;
        }
    }
}
```

## 第六步：开始对话

在 `PetController` 或需要触发对话的地方：

```csharp
using GalDialogueSystem;

// 开始剧情对话
if (GalDialogueManager.Instance != null && !string.IsNullOrEmpty(profile.startDialogueNodeID))
{
    GalDialogueManager.Instance.StartDialogue(profile.startDialogueNodeID);
}

// 开始气泡对话
if (GalDialogueManager.Instance != null && !string.IsNullOrEmpty(profile.touchDialogueNodeID))
{
    GalDialogueManager.Instance.StartDialogue(profile.touchDialogueNodeID);
}
```

## 第七步：测试

1. 运行游戏
2. 触发对话（点击宠物或调用开始对话的方法）
3. 检查对话是否正确显示
4. 测试选择功能
5. 测试历史记录（按H键显示）

## 常见问题

### Q: 对话不显示
A: 检查：
1. 对话数据库是否正确加载
2. 节点ID是否正确
3. UI预制体是否正确配置
4. Canvas层级是否正确设置

### Q: 资源加载失败
A: 检查：
1. 资源路径是否正确（相对于Resources文件夹）
2. 资源是否存在于Resources文件夹中
3. 资源文件扩展名是否正确

### Q: 选择项不显示
A: 检查：
1. 选择按钮预制体是否正确设置
2. 选择按钮容器是否正确配置
3. 选择项条件是否满足（如果有条件）

## 下一步

查看完整文档：`README.md`
