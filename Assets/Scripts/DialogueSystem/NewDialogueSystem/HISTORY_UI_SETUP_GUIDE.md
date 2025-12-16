# 历史对话UI配置指南

## 概述

历史对话UI允许玩家在剧情模式下查看所有对话记录，支持搜索、图片显示和时间点定位功能。

## 一、创建历史对话UI预制体

### 步骤1：创建根GameObject

1. 在Unity中创建空GameObject，命名为 `HistoryDialogueUI`
2. 添加 `HistoryDialogueUI` 脚本组件
3. 添加 `RectTransform` 组件（如果不存在）

### 步骤2：创建UI结构

#### 2.1 创建Panel（历史记录面板）

1. 右键 `HistoryDialogueUI` → `UI → Panel`
2. 将Panel命名为 `HistoryPanel`
3. 设置Panel的RectTransform：
   - Anchor: 全屏（Anchor Min: 0,0 | Anchor Max: 1,1）
   - Position: (0, 0, 0)
   - Size: 全屏
   - 背景颜色：半透明黑色（如：RGBA 0, 0, 0, 200）

#### 2.2 创建ScrollView（滚动视图）

1. 在 `HistoryPanel` 下创建 `UI → Scroll View`
2. 将ScrollView命名为 `HistoryScrollView`
3. 设置ScrollView的RectTransform：
   - Anchor: 居中，留边距
   - Position: (0, 0, 0)
   - Width: 1000-1200
   - Height: 600-800
4. 删除ScrollView自带的 `Viewport/Content` 下的示例内容

#### 2.3 配置Content（内容容器）

1. 展开 `HistoryScrollView/Viewport/Content`
2. 将Content命名为 `HistoryListContainer`（重要！脚本会通过名称查找）
3. 添加 `Vertical Layout Group` 组件到Content：
   - Spacing: 10
   - Child Alignment: Upper Center
   - Child Control Width: ✓
   - Child Control Height: ✗
   - Child Force Expand Width: ✓
   - Child Force Expand Height: ✗
   - Padding: Left 20, Right 20, Top 20, Bottom 20
4. 添加 `Content Size Fitter` 组件：
   - Horizontal Fit: Unconstrained
   - Vertical Fit: Preferred Size

#### 2.4 创建搜索栏

1. 在 `HistoryPanel` 下创建 `UI → Input Field - TextMeshPro`
2. 命名为 `SearchInputField`
3. 设置位置：顶部中央
4. 设置大小：Width 800, Height 40
5. Placeholder文本：`输入关键词搜索对话...`

6. 创建搜索按钮：
   - 在SearchInputField右侧创建 `UI → Button - TextMeshPro`
   - 命名为 `SearchButton`
   - 文本：`搜索`

7. 创建清除按钮：
   - 在SearchButton右侧创建 `UI → Button - TextMeshPro`
   - 命名为 `ClearSearchButton`
   - 文本：`清除`

#### 2.5 创建关闭按钮

1. 在 `HistoryPanel` 右上角创建 `UI → Button - TextMeshPro`
2. 命名为 `CloseButton`
3. 文本：`×` 或 `关闭`
4. 设置位置：右上角（如：X 950, Y 550）

### 步骤3：创建历史记录条目预制体（可选，推荐）

如果不创建预制体，脚本会使用默认样式。

#### 3.1 创建条目预制体

1. 在Project窗口创建空GameObject，命名为 `HistoryEntryPrefab`
2. 添加 `RectTransform` 组件
3. 设置RectTransform：
   - Width: 0（由Layout Group控制）
   - Height: 100（根据内容自适应）
   - Anchor: 顶部拉伸

#### 3.2 添加背景

1. 添加 `Image` 组件作为背景
2. 颜色：半透明（如：RGBA 50, 50, 50, 150）

#### 3.3 添加布局组件

1. 添加 `Vertical Layout Group`：
   - Spacing: 5
   - Padding: Left 10, Right 10, Top 10, Bottom 10
   - Child Control Width: ✓
   - Child Control Height: ✗

2. 添加 `Content Size Fitter`：
   - Vertical Fit: Preferred Size

#### 3.4 添加文本

1. 创建子对象 `Text`（UI → Text - TextMeshPro）
2. 设置文本样式：
   - Font Size: 16-18
   - Color: White
   - Alignment: Top Left
   - Enable Word Wrapping: ✓
   - Auto Size: ✓（可选）

#### 3.5 预留图片容器（可选）

如果需要显示图片，可以添加：

1. 创建子对象 `ImageContainer`（空GameObject）
2. 添加 `Horizontal Layout Group`：
   - Spacing: 10
   - Child Control Width: ✗
   - Child Control Height: ✗

3. 脚本会自动在ImageContainer中添加图片

#### 3.6 保存为预制体

将 `HistoryEntryPrefab` 拖拽到Project窗口保存为预制体

### 步骤4：配置HistoryDialogueUI脚本

在 `HistoryDialogueUI` GameObject的Inspector中配置：

#### 必须配置的字段：

- **History Panel**: 拖拽 `HistoryPanel` GameObject
- **History List Container**: 拖拽 `HistoryScrollView/Viewport/Content`（HistoryListContainer）
- **Close Button**: 拖拽 `CloseButton` GameObject

#### 可选配置的字段：

- **History Entry Prefab**: 拖拽创建的 `HistoryEntryPrefab` 预制体（如果不配置，会使用默认样式）
- **Search Input Field**: 拖拽 `SearchInputField` GameObject
- **Search Button**: 拖拽 `SearchButton` GameObject
- **Clear Search Button**: 拖拽 `ClearSearchButton` GameObject
- **Scrollbar**: 拖拽ScrollView的Scrollbar（可选，脚本会自动查找）
- **Scroll Rect**: 拖拽 `HistoryScrollView` GameObject（可选，脚本会自动查找）

#### 设置参数：

- **Max Display Entries**: 100（最大显示条目数）
- **Toggle Key**: H（打开/关闭历史UI的快捷键）

### 步骤5：配置DialogueSystemManager

1. 找到场景中的 `DialogueSystemManager` GameObject
2. 在Inspector中找到 `Default History Dialogue UI Prefab` 字段
3. 将创建的 `HistoryDialogueUI` 预制体拖拽到此字段

### 步骤6：确保Canvas配置正确

1. 确保 `HistoryDialogueUI` 位于一个Canvas下
2. 如果使用DialogueSystemManager自动创建的Canvas，确保SortingOrder设置正确（历史UI应该在最上层）
3. 建议为历史UI创建独立的Canvas，SortingOrder设为3000（确保在所有UI之上）

## 二、使用方法

### 2.1 打开历史UI

**方法1：快捷键（推荐）**
- 在剧情模式下按 `H` 键（可在Inspector中修改为其他键）

**方法2：代码调用**
```csharp
// 获取HistoryDialogueUI实例
IHistoryDialogueUI historyUI = DialogueSystemManager.Instance
    .GetOrCreateUI("history", DialogueDisplayMode.Custom, historyDialogueUIPrefab) 
    as IHistoryDialogueUI;

if (historyUI != null)
{
    historyUI.ShowHistory();
}
```

### 2.2 搜索功能

1. 在搜索输入框中输入关键词
2. 点击"搜索"按钮或按Enter键
3. 系统会搜索：
   - 对话文本内容
   - 角色名称
   - 节点ID

4. 点击"清除"按钮清除搜索，显示所有记录

### 2.3 时间点定位

**方法1：通过搜索定位**
- 输入关键词找到相关对话后，点击该条目即可查看

**方法2：代码跳转（高级）**
```csharp
HistoryDialogueUI historyUI = // 获取实例
System.DateTime targetTime = new System.DateTime(2024, 1, 1, 12, 30, 0);
historyUI.JumpToTimePoint(targetTime);
```

### 2.4 图片显示

历史记录会自动显示对话中包含的图片：

1. **背景图片**：如果对话节点有 `backgroundImagePath`，会在条目中显示
2. **插入图片**：如果对话节点有 `insertImagePaths`，会在条目下方的ImageContainer中横向排列显示

**注意事项**：
- 图片路径必须是相对于Resources文件夹的路径
- 例如：如果图片在 `Assets/Resources/Images/dialogue_bg.png`，路径应填写 `Images/dialogue_bg`

## 三、功能特性

### 3.1 自动记录

- 系统会自动记录所有剧情模式（DialogueDisplayMode.Story）下的对话
- 包括文本节点和图片节点的对话内容
- 记录包含：时间戳、角色名、对话文本、图片路径等信息

### 3.2 时间排序

- 历史记录按时间倒序排列（最新的在顶部）
- 每次打开历史UI时自动滚动到顶部（显示最新内容）

### 3.3 实时更新

- 如果历史UI已经打开，新的对话会自动添加到列表中
- 搜索过滤会在添加新条目时自动应用

### 3.4 性能优化

- 限制最大显示条目数（默认100条）
- 使用对象池管理条目（自动清理旧条目）
- 只在需要时刷新列表

## 四、常见问题

### Q1: 历史UI无法打开

**检查清单**：
- ✓ HistoryDialogueUI脚本是否正确添加到GameObject
- ✓ HistoryPanel是否正确配置
- ✓ DialogueSystemManager的Default History Dialogue UI Prefab是否正确设置
- ✓ 是否在剧情模式下（只有剧情模式的对话会被记录）

### Q2: 搜索功能不工作

**检查清单**：
- ✓ SearchInputField是否正确配置
- ✓ SearchButton的onClick事件是否正确绑定（脚本会自动绑定）
- ✓ 输入的关键词是否存在于对话中

### Q3: 图片不显示

**检查清单**：
- ✓ 图片路径是否正确（相对于Resources文件夹）
- ✓ 图片是否存在于Resources文件夹中
- ✓ DialogueNode的backgroundImagePath或insertImagePaths是否正确设置
- ✓ 图片格式是否支持（PNG、JPG等）

### Q4: 历史记录为空

**原因**：
- 只有在剧情模式（Story模式）下的对话才会被记录
- 气泡对话（Bubble模式）不会被记录

**解决方案**：
- 确保对话节点的displayMode设置为0（Story模式）

### Q5: 滚动条不工作

**检查清单**：
- ✓ ScrollView的Viewport和Content是否正确配置
- ✓ Content的RectTransform是否正确设置
- ✓ VerticalLayoutGroup是否正确配置
- ✓ Content Size Fitter是否正确设置

## 五、高级配置

### 5.1 自定义条目样式

1. 创建自定义的HistoryEntryPrefab
2. 确保预制体有Text组件（用于显示文本）
3. 可选：添加ImageContainer用于显示图片
4. 在HistoryDialogueUI脚本中指定此预制体

### 5.2 修改快捷键

在HistoryDialogueUI脚本的Inspector中修改 `Toggle Key` 字段，可以设置为任意KeyCode值。

### 5.3 调整显示数量

修改 `Max Display Entries` 参数（默认100），注意：
- 数值过大会影响性能
- 数值过小可能无法查看所有历史记录

### 5.4 添加时间点跳转按钮（高级）

如果需要添加时间点快速跳转功能，可以：

1. 创建时间点按钮UI
2. 在HistoryDialogueUI中添加跳转逻辑
3. 使用 `JumpToTimePoint(System.DateTime time)` 方法

## 六、代码示例

### 示例1：通过代码打开历史UI

```csharp
using NewDialogueSystem;

// 获取历史UI实例
GameObject historyUIPrefab = DialogueSystemManager.Instance.defaultHistoryDialogueUIPrefab;
if (historyUIPrefab != null)
{
    IHistoryDialogueUI historyUI = DialogueSystemManager.Instance
        .GetOrCreateUI("history", DialogueDisplayMode.Custom, historyUIPrefab) 
        as IHistoryDialogueUI;
    
    if (historyUI != null)
    {
        historyUI.ShowHistory();
    }
}
```

### 示例2：获取所有历史记录

```csharp
using NewDialogueSystem;

List<DialogueHistoryEntry> allHistory = DialogueSystemManager.Instance.GetHistory();

foreach (var entry in allHistory)
{
    Debug.Log($"[{entry.timestamp}] {entry.characterName}: {entry.text}");
}
```

### 示例3：清空历史记录

```csharp
DialogueSystemManager.Instance.ClearHistory();
```

## 七、注意事项

1. **性能考虑**：历史记录会占用内存，建议在适当时机清理（如章节结束）
2. **资源路径**：所有图片路径必须是相对于Resources文件夹的路径
3. **模式限制**：只有Story模式的对话会被记录，Bubble模式不会记录
4. **时间同步**：时间戳使用系统时间，确保系统时间正确
5. **Canvas层级**：建议历史UI使用独立的Canvas，SortingOrder设为最高值，确保显示在最上层

