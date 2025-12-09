using UnityEngine;
using UnityEditor;
using NewDialogueSystem;

/// <summary>
/// 编辑器工具：自动为ChoiceDialogueUI预制体添加脚本组件
/// </summary>
public class FixChoiceDialogueUIPrefab : EditorWindow
{
    [MenuItem("Tools/对话系统/修复ChoiceDialogueUI预制体")]
    public static void FixPrefab()
    {
        // 查找ChoiceDialogueUI预制体
        string[] guids = AssetDatabase.FindAssets("ChoiceDialogueUI t:Prefab");
        
        if (guids.Length == 0)
        {
            Debug.LogError("[FixChoiceDialogueUIPrefab] 找不到ChoiceDialogueUI预制体！");
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        if (prefab == null)
        {
            Debug.LogError($"[FixChoiceDialogueUIPrefab] 无法加载预制体: {assetPath}");
            return;
        }

        // 检查是否已有脚本组件
        ChoiceDialogueUI existingScript = prefab.GetComponent<ChoiceDialogueUI>();
        if (existingScript != null)
        {
            Debug.Log("[FixChoiceDialogueUIPrefab] ChoiceDialogueUI脚本已存在，无需修复");
            return;
        }

        // 添加脚本组件
        ChoiceDialogueUI script = prefab.AddComponent<ChoiceDialogueUI>();
        
        // 尝试自动配置引用
        AutoConfigureReferences(prefab, script);

        // 标记为已修改并保存
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[FixChoiceDialogueUIPrefab] ✓ 已为 {prefab.name} 添加ChoiceDialogueUI脚本组件");
        Debug.Log($"[FixChoiceDialogueUIPrefab] 请检查Inspector中的引用配置是否正确");
    }

    /// <summary>
    /// 自动配置脚本引用
    /// </summary>
    private static void AutoConfigureReferences(GameObject prefab, ChoiceDialogueUI script)
    {
        // 查找Panel
        if (script.choicePanel == null)
        {
            Transform panel = prefab.transform.Find("Panel");
            if (panel == null)
            {
                panel = FindChildRecursive(prefab.transform, "Panel");
            }
            if (panel != null)
            {
                script.choicePanel = panel.gameObject;
                Debug.Log("[FixChoiceDialogueUIPrefab] 自动配置choicePanel");
            }
        }

        // 查找按钮容器
        if (script.choiceButtonContainer == null)
        {
            Transform container = prefab.transform.Find("ChoiceButtonContainer");
            if (container == null)
            {
                container = FindChildRecursive(prefab.transform, "ChoiceButtonContainer");
            }
            if (container != null)
            {
                script.choiceButtonContainer = container;
                Debug.Log("[FixChoiceDialogueUIPrefab] 自动配置choiceButtonContainer");
            }
        }
    }

    /// <summary>
    /// 递归查找子对象
    /// </summary>
    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;

        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }

        return null;
    }
}

