using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器工具：在场景中创建DialogueSystemManager（备用方案）
/// </summary>
public class CreateDialogueSystemManager
{
    [MenuItem("Tools/创建 DialogueSystemManager")]
    public static void CreateDialogueSystemManagerInScene()
    {
        // 检查是否已存在
        GameObject existing = GameObject.Find("DialogueSystemManager");
        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog("提示", 
                "场景中已存在 DialogueSystemManager！\n\n是否要替换它？", 
                "替换", "取消");
            
            if (replace)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }
            else
            {
                Selection.activeGameObject = existing;
                return;
            }
        }

        // 创建新的GameObject
        GameObject dialogueManagerObj = new GameObject("DialogueSystemManager");
        
        // 尝试添加DialogueSystemManager组件
        System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
        
        if (dialogueSystemManagerType == null)
        {
            // 如果找不到，从所有程序集中查找
            Debug.Log("[CreateDialogueSystemManager] 直接查找失败，尝试从所有程序集中查找...");
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                if (dialogueSystemManagerType != null)
                {
                    Debug.Log($"[CreateDialogueSystemManager] 从程序集 {assembly.FullName} 找到类型");
                    break;
                }
            }
        }

        if (dialogueSystemManagerType != null)
        {
            try
            {
                dialogueManagerObj.AddComponent(dialogueSystemManagerType);
                EditorUtility.DisplayDialog("成功", "已创建 DialogueSystemManager！\n请检查 Hierarchy 面板。", "确定");
                Selection.activeGameObject = dialogueManagerObj;
                EditorUtility.SetDirty(dialogueManagerObj);
                Debug.Log("[CreateDialogueSystemManager] 成功创建 DialogueSystemManager GameObject");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"添加组件时发生错误：\n{e.Message}", "确定");
                Debug.LogError($"[CreateDialogueSystemManager] 添加组件失败: {e.Message}\n{e.StackTrace}");
                UnityEngine.Object.DestroyImmediate(dialogueManagerObj);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("错误", 
                "无法找到 NewDialogueSystem.DialogueSystemManager 类型！\n\n请检查：\n1. 脚本是否已编译\n2. 命名空间是否正确", 
                "确定");
            Debug.LogError("[CreateDialogueSystemManager] 无法找到 DialogueSystemManager 类型");
            
            // 列出所有程序集用于调试
            Debug.Log("[CreateDialogueSystemManager] 已加载的程序集：");
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                Debug.Log($"  - {assembly.FullName}");
            }
            
            UnityEngine.Object.DestroyImmediate(dialogueManagerObj);
        }
    }
}

