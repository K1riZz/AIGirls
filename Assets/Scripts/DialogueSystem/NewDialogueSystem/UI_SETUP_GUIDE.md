# UI预制体制作指南 - Unity 2022.3

本指南详细说明如何在Unity 2022.3中创建对话系统的UI预制体。

## Unity 2022.3 UI创建方式

在Unity 2022.3中，使用UI菜单创建UI元素时，Unity会自动创建一个Canvas作为父对象。这是标准的UI创建方式，新对话系统完全支持这种方式。

## 一、创建剧情对话UI（StoryDialogueUI）

### 步骤1：创建根GameObject

1. 在Project窗口中，右键点击要存放预制体的文件夹
2. 选择 `Create → Empty`
3. 命名为 `StoryDialogueUI`

### 步骤2：添加脚本组件

1. 选中 `StoryDialogueUI` GameObject
2. 在Inspector中点击 `Add Component`
3. 搜索并添加 `StoryDialogueUI` 脚本
4. 添加 `Canvas Group` 组件（Unity UI）

### 步骤3：创建Canvas和Panel

1. 选中 `StoryDialogueUI` GameObject
2. 右键点击 → `UI → Panel`
   - Unity会自动创建一个Canvas作为父对象
   - Canvas下会创建一个Panel
3. 层级结构应该是：
   ```
   StoryDialogueUI
   ├── StoryDialogueUI 脚本
   ├── Canvas Group 组件
   └── Canvas (Unity自动创建)
       ├── Canvas 组件
       ├── CanvasScaler 组件
       ├── GraphicRaycaster 组件
       └── Panel
   ```

### 步骤4：配置Canvas

1. 选中Canvas GameObject
2. 设置Canvas组件：
   - **Render Mode**: `Screen Space - Overlay`
   - **Sorting Order**: `200` (确保在桌宠UI之上)

### 步骤5：在Panel下创建UI元素

选中Panel，然后依次创建：

1. **角色名称文本**：
   - 右键Panel → `UI → TextMeshPro - Text (UI)`
   - 命名为 `CharacterNameText`
   - 设置字体大小、对齐方式等

2. **对话文本**：
   - 右键Panel → `UI → TextMeshPro - Text (UI)`
   - 命名为 `DialogueText`
   - 设置为多行文本，设置合适的字体大小

3. **角色立绘Image**（可选）：
   - 右键Panel → `UI → Image`
   - 命名为 `CharacterPortrait`
   - 设置Image类型为 `Simple` 或 `Sliced`

4. **背景Image**（可选）：
   - 右键Panel → `UI → Image`
   - 命名为 `BackgroundImage`
   - 设置Image类型

### 步骤6：配置StoryDialogueUI脚本

1. 选中根 `StoryDialogueUI` GameObject
2. 在Inspector中找到 `StoryDialogueUI` 脚本
3. 拖拽配置各个字段：
   - **Dialogue Panel**: 拖入Canvas下的Panel
   - **Character Name Text**: 拖入CharacterNameText
   - **Dialogue Text**: 拖入DialogueText
   - **Character Portrait**: 拖入CharacterPortrait（如果有）
   - **Background Image**: 拖入BackgroundImage（如果有）

### 步骤7：保存为预制体

1. 将整个 `StoryDialogueUI` GameObject拖到Project窗口中
2. 删除场景中的实例（如果是在场景中创建的）

## 二、创建气泡对话UI（BubbleDialogueUI）

### 步骤1：创建根GameObject

1. 创建Empty GameObject，命名为 `BubbleDialogueUI`
2. 添加 `BubbleDialogueUI` 脚本
3. 添加 `Canvas Group` 组件

### 步骤2：创建Panel和文本

1. 右键 `BubbleDialogueUI` → `UI → Panel`
2. Unity自动创建Canvas和Panel
3. 选中Panel，右键 → `UI → TextMeshPro - Text (UI)`
4. 命名为 `BubbleText`

### 步骤3：配置Canvas

1. 选中Canvas
2. 设置 `Render Mode` 为 `Screen Space - Overlay`
3. 设置 `Sorting Order` 为 `200`

### 步骤4：配置BubbleDialogueUI脚本

1. 选中根 `BubbleDialogueUI` GameObject
2. 在Inspector中配置：
   - **Bubble Panel**: 拖入Panel
   - **Bubble Text**: 拖入BubbleText

### 步骤5：保存为预制体

将GameObject拖到Project窗口保存为预制体。

## 三、创建选择对话UI（ChoiceDialogueUI）

### 步骤1：创建根GameObject

1. 创建Empty GameObject，命名为 `ChoiceDialogueUI`
2. 添加 `ChoiceDialogueUI` 脚本

### 步骤2：创建UI结构

1. 右键 `ChoiceDialogueUI` → `UI → Panel`
2. Panel下创建：
   - **ScrollView** 或 **Vertical Layout Group**（用于容纳选择按钮）
   - 创建Button作为选择按钮的预制体：
     - 右键Panel → `UI → Button - TextMeshPro`
     - 命名为 `ChoiceButtonPrefab`
     - 配置按钮样式
     - 保存为预制体

### 步骤3：配置ChoiceDialogueUI脚本

1. **Choice Panel**: Panel GameObject
2. **Choice Button Prefab**: ChoiceButtonPrefab预制体
3. **Choice Button Container**: ScrollView或Layout Group的Content区域

### 步骤4：保存为预制体

保存整个结构为预制体。

## 四、创建历史记录UI（HistoryDialogueUI）

### 步骤1：创建根GameObject

1. 创建Empty GameObject，命名为 `HistoryDialogueUI`
2. 添加 `HistoryDialogueUI` 脚本

### 步骤2：创建UI结构

1. 右键 `HistoryDialogueUI` → `UI → Panel`
2. Panel下创建：
   - **ScrollView** (用于显示历史记录列表)
   - 创建历史记录条目预制体：
     - 右键 → `UI → Panel`
     - 命名为 `HistoryEntryPrefab`
     - 添加文本组件显示对话内容
   - **关闭按钮**: `UI → Button - TextMeshPro`

### 步骤3：配置脚本

配置HistoryDialogueUI脚本的各个字段引用。

### 步骤4：保存为预制体

保存为预制体。

## 五、常见问题

### Q: Canvas层级问题

A: 确保Canvas的 `Sorting Order` 设置正确：
- 桌宠UI: 100
- 对话UI: 200
- 其他顶层UI: 300+

### Q: UI不显示

A: 检查：
1. Canvas的Render Mode是否正确
2. Canvas是否激活
3. Panel是否激活
4. CanvasGroup的alpha是否为1
5. UI是否在屏幕范围内

### Q: 文本不显示

A: 检查：
1. TextMeshPro组件是否正确配置
2. 文本颜色是否可见（不是透明或与背景色相同）
3. 字体资源是否正确加载

### Q: 按钮不响应

A: 检查：
1. GraphicRaycaster组件是否存在于Canvas上
2. EventSystem是否存在于场景中
3. 按钮的Interactable是否为true

## 六、最佳实践

1. **使用Layout Group**: 使用Vertical Layout Group或Horizontal Layout Group自动排列UI元素
2. **使用Content Size Fitter**: 自动调整UI元素大小
3. **使用Canvas Scaler**: 确保UI在不同分辨率下正确缩放
4. **设置合理的Anchor和Pivot**: 确保UI在不同屏幕尺寸下正确定位

## 七、完整的示例层级结构

```
StoryDialogueUI (根GameObject)
├── StoryDialogueUI 脚本
├── Canvas Group 组件
└── Canvas (Unity自动创建)
    ├── Canvas 组件 (Render Mode: Screen Space - Overlay, Sorting Order: 200)
    ├── CanvasScaler 组件
    ├── GraphicRaycaster 组件
    └── Panel (Image组件)
        ├── CharacterNameText (TextMeshProUGUI)
        ├── DialogueText (TextMeshProUGUI)
        ├── CharacterPortrait (Image)
        └── BackgroundImage (Image)
```

这就是Unity 2022.3标准UI创建方式下的完整结构！

