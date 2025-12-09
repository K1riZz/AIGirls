using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 编辑器工具：查找所有使用 DontDestroyOnLoad 的对象
/// 这些对象在运行时存在，但在 Hierarchy 中可能不可见（因为它们在特殊的 DDOL 场景中）
/// </summary>
public class FindDontDestroyOnLoadObjects
{
    [MenuItem("Tools/查找 DontDestroyOnLoad 对象")]
    public static void FindDDOLObjects()
    {
        // 在运行时查找所有 GameObject
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "此工具只能在运行时使用！\n\n请先运行游戏，然后再使用此工具。", "确定");
            return;
        }

        List<GameObject> ddolObjects = new List<GameObject>();

        // 查找所有场景中的对象
        UnityEngine.SceneManagement.Scene ddolScene = default(UnityEngine.SceneManagement.Scene);
        
        // Unity 会为 DontDestroyOnLoad 对象创建一个特殊的场景
        // 我们需要查找这个场景中的对象
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            // DDOL 场景通常没有有效的路径或名称很特殊
            if (scene.isLoaded && (string.IsNullOrEmpty(scene.path) || scene.name == "DontDestroyOnLoad"))
            {
                ddolScene = scene;
                break;
            }
        }

        // 如果找到了 DDOL 场景，列出其中的所有对象
        if (ddolScene.IsValid())
        {
            GameObject[] rootObjects = ddolScene.GetRootGameObjects();
            Debug.Log($"[FindDDOLObjects] 找到 DontDestroyOnLoad 场景：{ddolScene.name}，包含 {rootObjects.Length} 个根对象");
            
            foreach (var obj in rootObjects)
            {
                ddolObjects.Add(obj);
                Debug.Log($"[FindDDOLObjects]   - {obj.name} (激活: {obj.activeSelf}, 场景: {obj.scene.name})");
            }
        }

        // 也尝试通过 FindObjectsOfType 查找已知的管理器
        var gameManager = Object.FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log($"[FindDDOLObjects] 通过 FindObjectOfType 找到 GameManager: {gameManager.name}, 场景: {gameManager.gameObject.scene.name}");
        }
        else
        {
            Debug.LogWarning("[FindDDOLObjects] 未找到 GameManager！");
        }

        // 尝试通过反射查找 DialogueSystemManager
        System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
        if (dialogueSystemManagerType == null)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                if (dialogueSystemManagerType != null) break;
            }
        }

        if (dialogueSystemManagerType != null)
        {
            var dialogueManager = Object.FindObjectOfType(dialogueSystemManagerType);
            if (dialogueManager != null)
            {
                Debug.Log($"[FindDDOLObjects] 通过 FindObjectOfType 找到 DialogueSystemManager: {dialogueManager.name}, 场景: {(dialogueManager as MonoBehaviour).gameObject.scene.name}");
            }
            else
            {
                Debug.LogWarning("[FindDDOLObjects] 未找到 DialogueSystemManager！");
            }

            // 也检查 Instance 属性
            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    var go = (instance as MonoBehaviour).gameObject;
                    Debug.Log($"[FindDDOLObjects] 通过 Instance 属性找到 DialogueSystemManager: {go.name}, 场景: {go.scene.name}");
                }
                else
                {
                    Debug.LogWarning("[FindDDOLObjects] DialogueSystemManager.Instance 为 null！");
                }
            }
        }

        // 显示汇总信息
        string summary = $"找到 {ddolObjects.Count} 个 DontDestroyOnLoad 对象。\n\n";
        summary += "详细信息请查看控制台日志。\n\n";
        summary += "注意：这些对象在 Hierarchy 中可能不可见，因为它们在特殊的 DDOL 场景中。";
        
        EditorUtility.DisplayDialog("查找结果", summary, "确定");
    }

    [MenuItem("Tools/验证管理器状态")]
    public static void VerifyManagers()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "此工具只能在运行时使用！", "确定");
            return;
        }

        string report = "=== 管理器状态报告 ===\n\n";

        // 检查 GameManager
        if (GameManager.Instance != null)
        {
            report += $"✓ GameManager.Instance 存在\n";
            report += $"  - GameObject名称: {GameManager.Instance.gameObject.name}\n";
            report += $"  - 场景: {GameManager.Instance.gameObject.scene.name}\n";
            report += $"  - 激活状态: {GameManager.Instance.gameObject.activeSelf}\n\n";
        }
        else
        {
            report += "✗ GameManager.Instance 为 null\n\n";
        }

        // 检查 DialogueSystemManager
        System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
        if (dialogueSystemManagerType == null)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                if (dialogueSystemManagerType != null) break;
            }
        }

        if (dialogueSystemManagerType != null)
        {
            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    var go = (instance as MonoBehaviour).gameObject;
                    report += $"✓ DialogueSystemManager.Instance 存在\n";
                    report += $"  - GameObject名称: {go.name}\n";
                    report += $"  - 场景: {go.scene.name}\n";
                    report += $"  - 激活状态: {go.activeSelf}\n\n";
                }
                else
                {
                    report += "✗ DialogueSystemManager.Instance 为 null\n\n";
                }
            }
        }
        else
        {
            report += "✗ 无法找到 DialogueSystemManager 类型\n\n";
        }

        Debug.Log(report);
        EditorUtility.DisplayDialog("管理器状态", report, "确定");
    }
}

