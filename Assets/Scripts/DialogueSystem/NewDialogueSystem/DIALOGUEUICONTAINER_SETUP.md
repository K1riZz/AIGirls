# DialogueUIContainer 配置说明

## 概述
`DialogueUIContainer` 是所有对话UI的父容器，用于统一管理所有对话UI实例的位置和层级。

## 配置方式

### 方式 1：自动创建（推荐）
**什么都不做！** 如果 `DialogueUIContainer` 字段在 Inspector 中保持为 `None` 或 `null`，系统会在 `DialogueSystemManager.Awake()` 中自动创建。

自动创建的容器包含：
- Canvas 组件（`ScreenSpaceOverlay` 模式）
- CanvasScaler 组件（适配 1920x1080 分辨率）
- GraphicRaycaster 组件
- 自动设置为 UI 层
- sortingOrder 设为 200（确保在所有桌宠UI之上）

### 方式 2：手动配置
如果你已经有自己的 UI Canvas 结构，可以手动指定：

1. 在场景中创建一个 Canvas（或使用现有的）
2. 在 `DialogueSystemManager` 组件的 Inspector 中
3. 将 `DialogueUIContainer` 字段拖拽到你的 Canvas Transform
4. 系统会使用你指定的容器，不会自动创建

### 手动配置要求

如果你手动配置容器，建议满足以下条件：

- **Canvas 组件**：容器或其父对象必须有 Canvas 组件
- **Render Mode**：建议使用 `ScreenSpaceOverlay` 或 `ScreenSpaceCamera`
- **Sorting Order**：建议设置为较高值（如 200）以确保对话UI显示在最上层
- **CanvasScaler**：建议添加以适配不同分辨率
- **GraphicRaycaster**：如果需要UI交互（如点击按钮），需要添加此组件

### 验证配置

运行游戏后，检查 Hierarchy：
- 如果看到 `DialogueUIContainer` GameObject 自动创建在 `DialogueSystemManager` 下，说明自动创建成功
- 如果使用手动配置，检查控制台是否有警告或错误信息

### 常见问题

**Q: 对话UI不显示，怎么办？**
A: 检查 `DialogueUIContainer` 是否激活，其 Canvas 是否启用，sortingOrder 是否足够高。

**Q: 可以删除自动创建的容器吗？**
A: 可以，但需要先手动指定另一个容器，否则系统会重新创建。

**Q: 容器必须是 Canvas 吗？**
A: 容器本身不一定是 Canvas，但必须有 Canvas 作为父对象或组件，因为对话UI需要 Canvas 才能渲染。

**Q: 多个对话可以共享同一个容器吗？**
A: 是的，所有对话UI实例都会在同一个容器下创建和管理。

## 代码位置

相关代码在 `DialogueSystemManager.cs` 的 `InitializeUIContainer()` 方法中：

```csharp
private void InitializeUIContainer()
{
    if (dialogueUIContainer == null)
    {
        // 自动创建容器...
    }
}
```

