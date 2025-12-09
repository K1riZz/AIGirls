# 快速开始指南

本文档帮助你快速上手新对话系统。

## 一、系统要求

- Unity 2022.3.42f1c1 或更高版本
- TextMeshPro（通常Unity已包含）

## 二、安装步骤

### 1. 复制文件

确保 `NewDialogueSystem` 文件夹在 `Assets/Scripts/DialogueSystem/` 目录下。

### 2. 创建必要目录

在 `Assets/Resources/` 目录下创建 `Dialogue/` 文件夹（如果没有的话）。

## 三、快速开始（5分钟）

### 步骤1：创建对话数据库JSON

1. 在 `Assets/Resources/Dialogue/` 目录下创建 `database.json` 文件
2. 复制以下内容：

```json
{
    "databaseID": "main_database",
    "databaseName": "主对话数据库",
    "version": "1.0",
    "nodes": [
        {
            "nodeID": "start",
            "nodeType": 0,
            "displayMode": 1,
            "characterID": "pet",
            "text": "你好！这是气泡对话！",
            "textSpeed": 30.0,
            "autoAdvanceTime": 3.0,
            "nextNodeID": "end"
        },
        {
            "nodeID": "end",
            "nodeType": 5
        }
    ],
    "characters": [
        {
            "characterID": "pet",
            "characterName": "小宠物",
            "nameColor": {"r": 1.0, "g": 1.0, "b": 1.0, "a": 1.0},
            "textColor": {"r": 1.0, "g": 1.0, "b": 1.0, "a": 1.0},
            "voiceVolume": 1.0,
            "voicePitch": 1.0,
            "defaultTextSpeed": 30.0
        }
    ],
    "groups": []
}
```

### 步骤2：创建DialogueSystemManager

1. 在场景中创建空GameObject，命名为 `DialogueSystemManager`
2. 添加 `DialogueSystemManager` 组件（NewDialogueSystem命名空间）
3. 设置 `Database Json Path` 为：`Dialogue/database`

### 步骤3：创建气泡对话UI预制体（最简单版本）

1. **创建根对象**：
   - Project窗口右键 → Create Empty
   - 命名为 `BubbleDialogueUI`

2. **添加组件**：
   - Add Component → `BubbleDialogueUI` (NewDialogueSystem)
   - Add Component → `Canvas Group`

3. **创建UI结构**：
   - 右键 `BubbleDialogueUI` → UI → Panel
   - Unity自动创建Canvas和Panel
   - 选中Panel，右键 → UI → TextMeshPro - Text (UI)
   - 命名为 `BubbleText`

4. **配置脚本**：
   - 选中 `BubbleDialogueUI`
   - 在Inspector中：
     - **Bubble Panel**: 拖入Canvas下的Panel
     - **Bubble Text**: 拖入BubbleText

5. **保存为预制体**：
   - 将 `BubbleDialogueUI` 拖到Project窗口
   - 删除场景中的实例

6. **配置Manager**：
   - 选中场景中的 `DialogueSystemManager`
   - 将 `BubbleDialogueUI` 预制体拖到 `Default Bubble Dialogue UI Prefab` 字段

### 步骤4：启动对话

创建一个测试脚本：

```csharp
using UnityEngine;
using NewDialogueSystem;

public class TestDialogue : MonoBehaviour
{
    void Start()
    {
        // 等待一帧确保系统初始化完成
        Invoke(nameof(StartDialogue), 0.1f);
    }

    void StartDialogue()
    {
        DialogueSystemManager.Instance.StartDialogue("start");
    }
}
```

将脚本添加到场景中的任意GameObject上，运行游戏即可看到对话！

## 四、下一步

- 查看 `README.md` 了解完整功能
- 查看 `UI_SETUP_GUIDE.md` 学习创建更复杂的UI
- 学习如何创建选择对话、历史记录等功能

## 五、常见问题

### Q: 对话不显示？

A: 检查：
1. DialogueSystemManager是否在场景中
2. Database Json Path是否正确
3. UI预制体是否配置到Manager中
4. 查看Console错误信息

### Q: JSON格式错误？

A: 确保JSON格式正确，可以使用在线JSON验证器检查。

## 完成！

现在你已经成功设置了新对话系统！🎉

