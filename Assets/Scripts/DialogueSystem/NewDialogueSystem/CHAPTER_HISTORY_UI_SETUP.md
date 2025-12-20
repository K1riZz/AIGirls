# 章节式历史对话UI配置指南

## 概述

历史对话UI已改为章节式显示：
- **左侧/顶部**：章节按钮列表（只显示已解锁的章节）
- **右侧/下方**：选中章节的对话列表（显示该章节的所有对话记录）

## 一、数据结构配置

### 1.1 创建章节数据

#### 方法1：使用ScriptableObject（推荐）

1. 创建ScriptableObject脚本 `ChapterDatabaseSO.cs`：
```csharp
using UnityEngine;
using NewDialogueSystem;

[CreateAssetMenu(fileName = "ChapterDatabase", menuName = "Dialogue/Chapter Database")]
public class ChapterDatabaseSO : ScriptableObject
{
    public ChapterDatabase chapterDatabase;
}
```

2. 在Unity中创建ScriptableObject实例：
   - 右键Project窗口 → `Create → Dialogue → Chapter Database`
   - 命名为 `ChapterDatabase`

3. 在Inspector中配置章节列表：
   - **Chapter ID**: 章节唯一标识（如：`chapter_001`）
   - **Chapter Name**: 章节显示名称（如：`第一章：初遇`）
   - **Description**: 章节描述（可选）
   - **Icon Path**: 章节图标路径（可选）
   - **Order**: 章节顺序（用于排序，数字越小越靠前）

#### 方法2：通过JSON配置

创建JSON文件 `chapters.json`：
```json
{
  "chapters": [
    {
      "chapterID": "chapter_001",
      "chapterName": "第一章：初遇",
      "description": "初次相遇的故事",
      "iconPath": "Icons/chapter_001",
      "order": 0
    },
    {
      "chapterID": "chapter_002",
      "chapterName": "第二章：相识",
      "description": "深入了解的过程",
      "iconPath": "Icons/chapter_002",
      "order": 1
    }
  ]
}
```

然后在DialogueSystemManager中加载JSON（需要实现加载器）。

### 1.2 配置对话节点关联章节

在对话节点的JSON配置中添加 `chapterID` 字段：

```json
{
  "nodeID": "dialogue_001",
  "chapterID": "chapter_001",
  "text": "这是第一章的对话内容",
  "characterID": "character_001",
  "displayMode": 0
}
```

**注意**：
- `chapterID` 必须与ChapterDatabase中定义的章节ID一致
- 如果对话节点没有 `chapterID`，该对话不会被分到任何章节
- 只有 `displayMode` 为 0（Story模式）的对话才会被记录到历史

## 二、UI结构配置

### 2.1 创建历史对话UI预制体

#### 步骤1：创建根GameObject

1. 创建空GameObject，命名为 `HistoryDialogueUI`
2. 添加 `HistoryDialogueUI` 脚本组件

#### 步骤2：创建Panel

1. 创建 `UI → Panel`，命名为 `HistoryPanel`
2. 设置全屏覆盖（Anchor: 0,0 → 1,1）
3. 背景颜色：半透明黑色（RGBA 0, 0, 0, 200）

#### 步骤3：创建章节按钮容器

**选项A：左侧布局（推荐）**

1. 在 `HistoryPanel` 下创建空GameObject，命名为 `ChapterButtonContainer`
2. 添加 `RectTransform` 组件
3. 设置位置和大小：
   - Anchor: 左侧（Anchor Min: 0,0 | Anchor Max: 0,1）
   - Width: 250-300
   - Position X: 125-150（居中）
4. 添加 `Vertical Layout Group`：
   - Spacing: 10
   - Padding: Top 20, Bottom 20, Left 10, Right 10
   - Child Alignment: Upper Center
   - Child Control Width: ✓
   - Child Control Height: ✗

**选项B：顶部布局**

1. 在 `HistoryPanel` 下创建空GameObject，命名为 `ChapterButtonContainer`
2. 添加 `RectTransform` 组件
3. 设置位置和大小：
   - Anchor: 顶部（Anchor Min: 0,1 | Anchor Max: 1,1）
   - Height: 80-100
   - Position Y: -40 到 -50
4. 添加 `Horizontal Layout Group`：
   - Spacing: 10
   - Padding: Left 20, Right 20, Top 10, Bottom 10
   - Child Alignment: Middle Center
   - Child Control Width: ✗
   - Child Control Height: ✓

#### 步骤4：创建对话列表（ScrollView）

1. 在 `HistoryPanel` 下创建 `UI → Scroll View`
2. 命名为 `DialogueScrollView`
3. 设置位置和大小：
   - **如果章节按钮在左侧**：Anchor Min: (0.2, 0) | Anchor Max: (1, 1)，留出左侧空间
   - **如果章节按钮在顶部**：Anchor Min: (0, 0.1) | Anchor Max: (1, 1)，留出顶部空间
4. 展开 `DialogueScrollView/Viewport/Content`
5. 将Content命名为 `DialogueListContainer`（重要！）
6. 在Content上添加 `Vertical Layout Group`：
   - Spacing: 10
   - Padding: Left 20, Right 20, Top 20, Bottom 20
   - Child Alignment: Upper Center
   - Child Control Width: ✓
   - Child Control Height: ✗
7. 在Content上添加 `Content Size Fitter`：
   - Vertical Fit: Preferred Size

#### 步骤5：创建关闭按钮

1. 在 `HistoryPanel` 右上角创建 `UI → Button - TextMeshPro`
2. 命名为 `CloseButton`
3. 文本：`×` 或 `关闭`
4. 位置：右上角（如：X 950, Y 550）

### 2.2 创建章节按钮预制体（可选，推荐）

如果不创建预制体，脚本会使用默认样式。

#### 步骤1：创建章节按钮

1. 创建 `UI → Button - TextMeshPro`
2. 命名为 `ChapterButtonPrefab`
3. 设置样式：
   - 宽度：200-250（如果垂直布局）
   - 高度：50-60
   - 背景颜色：深灰色（RGBA 50, 50, 50, 200）
   - 文本大小：18-20
   - 文本对齐：居中

#### 步骤2：保存为预制体

将 `ChapterButtonPrefab` 拖拽到Project窗口保存为预制体。

### 2.3 创建对话条目预制体（可选）

如果不创建预制体，脚本会使用默认样式。参考之前的配置指南。

## 三、脚本配置

### 3.1 配置DialogueSystemManager

1. 找到场景中的 `DialogueSystemManager` GameObject
2. 在Inspector中配置：
   - **Chapter Database**: 拖拽创建的ChapterDatabase ScriptableObject
   - **Default History Dialogue UI Prefab**: 拖拽创建的HistoryDialogueUI预制体

### 3.2 配置HistoryDialogueUI脚本

在 `HistoryDialogueUI` GameObject的Inspector中配置：

#### 必须配置的字段：

- **History Panel**: 拖拽 `HistoryPanel` GameObject
- **Chapter Button Container**: 拖拽 `ChapterButtonContainer` GameObject
- **Dialogue List Container**: 拖拽 `DialogueScrollView/Viewport/Content`（DialogueListContainer）
- **Close Button**: 拖拽 `CloseButton` GameObject

#### 可选配置的字段：

- **Chapter Button Prefab**: 拖拽创建的章节按钮预制体（不配置则使用默认样式）
- **History Entry Prefab**: 拖拽创建的对话条目预制体（不配置则使用默认样式）
- **Scroll Rect**: 拖拽 `DialogueScrollView` GameObject（脚本会自动查找）

#### 设置参数：

- **Max Display Entries**: 100（每个章节最大显示条目数）
- **Toggle Key**: H（打开/关闭历史UI的快捷键）

## 四、使用流程

### 4.1 章节解锁机制

**自动解锁**：
- 当玩家访问某个章节的对话时，该章节自动解锁
- 解锁状态保存在 `DialogueSystemManager` 的 `unlockedChapters` 中

**解锁条件**：
- 对话节点必须设置了 `chapterID`
- 对话节点的 `displayMode` 必须为 0（Story模式）
- 对话被显示时，对应章节自动解锁

### 4.2 查看历史对话

1. **打开历史UI**：
   - 在剧情模式下按 `H` 键（或配置的其他快捷键）
   
2. **选择章节**：
   - 左侧（或顶部）显示所有已解锁的章节按钮
   - 点击章节按钮查看该章节的所有对话
   - **只有已解锁的章节才会显示**

3. **查看对话**：
   - 右侧（或下方）显示选中章节的所有对话记录
   - 对话按时间倒序排列（最新的在顶部）
   - 支持滚动查看所有对话
   - 显示图片（背景图和插入图）

4. **关闭历史UI**：
   - 点击关闭按钮或再次按 `H` 键

## 五、功能特性

### 5.1 章节管理

- ✅ **自动分组**：对话根据 `chapterID` 自动分组到对应章节
- ✅ **自动解锁**：访问章节对话后自动解锁该章节
- ✅ **顺序显示**：章节按 `order` 字段排序显示
- ✅ **只显示已解锁**：未解锁的章节不会显示在列表中

### 5.2 对话显示

- ✅ **时间戳**：每条对话显示完整时间戳
- ✅ **图片支持**：显示背景图片和插入图片
- ✅ **时间排序**：对话按时间倒序排列
- ✅ **自动滚动**：切换章节时自动滚动到顶部（最新内容）

### 5.3 用户体验

- ✅ **简洁UI**：章节按钮和对话列表分离，界面清晰
- ✅ **快速切换**：点击章节按钮即可切换查看不同章节
- ✅ **快捷键支持**：按H键快速打开/关闭

## 六、配置示例

### 示例1：完整配置流程

```csharp
// 1. 创建章节数据（在ScriptableObject或JSON中）
ChapterData chapter1 = new ChapterData("chapter_001", "第一章：初遇", 0);
ChapterData chapter2 = new ChapterData("chapter_002", "第二章：相识", 1);

// 2. 在对话节点中关联章节
{
  "nodeID": "dialogue_001",
  "chapterID": "chapter_001",  // 关联到第一章
  "text": "这是第一章的对话",
  "displayMode": 0
}

// 3. 玩家访问对话后，chapter_001自动解锁
// 4. 历史UI中会显示"第一章：初遇"按钮
// 5. 点击按钮查看该章节的所有对话
```

### 示例2：检查章节是否解锁

```csharp
using NewDialogueSystem;

// 检查章节是否已解锁
bool isUnlocked = DialogueSystemManager.Instance.IsChapterUnlocked("chapter_001");

// 获取所有已解锁的章节
HashSet<string> unlocked = DialogueSystemManager.Instance.GetUnlockedChapters();
```

### 示例3：获取章节的历史记录

```csharp
// 获取指定章节的所有历史记录
List<DialogueHistoryEntry> chapterHistory = 
    DialogueSystemManager.Instance.GetHistoryByChapter("chapter_001");
```

## 七、常见问题

### Q1: 章节按钮不显示

**检查清单**：
- ✓ ChapterDatabase是否正确配置
- ✓ 对话节点是否设置了chapterID
- ✓ 是否已经访问过该章节的对话（章节需要解锁）
- ✓ ChapterButtonContainer是否正确配置

### Q2: 点击章节按钮没有反应

**检查清单**：
- ✓ DialogueListContainer是否正确配置
- ✓ ScrollRect是否正确配置
- ✓ 该章节是否有历史记录

### Q3: 对话没有分到章节

**原因**：
- 对话节点的chapterID字段未设置或为空
- chapterID与ChapterDatabase中定义的ID不匹配

**解决方案**：
- 检查对话节点的chapterID字段
- 确保ChapterDatabase中定义了对应的章节

### Q4: 未解锁的章节显示了

**原因**：
- 章节解锁机制可能有bug，或者章节ID为空字符串

**解决方案**：
- 检查DialogueSystemManager的解锁逻辑
- 确保只有访问过的章节才会解锁

## 八、UI布局建议

### 布局1：左侧章节，右侧对话（推荐）

```
┌─────────────────────────────────────────┐
│ HistoryPanel                            │
│ ┌──────┐  ┌──────────────────────────┐ │
│ │章节1 │  │ [时间]角色:对话内容       │ │
│ │章节2 │  │ [时间]角色:对话内容       │ │
│ │章节3 │  │ [时间]角色:对话内容       │ │
│ │      │  │ ...                      │ │
│ │      │  │                          │ │
│ └──────┘  └──────────────────────────┘ │
└─────────────────────────────────────────┘
```

### 布局2：顶部章节，下方对话

```
┌─────────────────────────────────────────┐
│ HistoryPanel                            │
│ [章节1] [章节2] [章节3] ...            │
│ ┌────────────────────────────────────┐ │
│ │ [时间]角色:对话内容                 │ │
│ │ [时间]角色:对话内容                 │ │
│ │ ...                                │ │
│ └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## 九、高级配置

### 9.1 自定义章节按钮样式

创建自定义的ChapterButtonPrefab，确保：
- 有Button组件
- 有TextMeshProUGUI组件用于显示章节名称
- 可以添加Image组件用于显示章节图标

### 9.2 章节图标

如果ChapterData中设置了iconPath：
1. 在章节按钮预制体中添加Image组件
2. 在CreateChapterButton方法中加载并显示图标
3. 或者在预制体中手动配置图标Image

### 9.3 章节描述

可以在章节按钮上添加Tooltip或悬停提示，显示章节描述信息。

详细配置步骤已完成，所有代码已通过编译检查！


