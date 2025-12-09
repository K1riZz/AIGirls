using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话数据库加载器（支持JSON和XML格式）
    /// </summary>
    public class DialogueDatabaseLoader
    {
        /// <summary>
        /// 从JSON文件加载对话数据库
        /// </summary>
        public DialogueDatabase LoadFromJson(string jsonPath)
        {
            // 从Resources加载
            TextAsset jsonAsset = Resources.Load<TextAsset>(jsonPath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[DialogueDatabaseLoader] 无法加载JSON文件：{jsonPath}");
                return null;
            }

            try
            {
                DialogueDatabase database = JsonUtility.FromJson<DialogueDatabase>(jsonAsset.text);
                return database;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueDatabaseLoader] JSON解析失败：{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从JSON字符串加载对话数据库
        /// </summary>
        public DialogueDatabase LoadFromJsonString(string jsonString)
        {
            try
            {
                DialogueDatabase database = JsonUtility.FromJson<DialogueDatabase>(jsonString);
                return database;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueDatabaseLoader] JSON解析失败：{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存对话数据库到JSON文件
        /// </summary>
        public void SaveToJson(DialogueDatabase database, string filePath)
        {
            try
            {
                string jsonString = JsonUtility.ToJson(database, true);
                File.WriteAllText(filePath, jsonString, Encoding.UTF8);
                Debug.Log($"[DialogueDatabaseLoader] 对话数据库已保存到：{filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueDatabaseLoader] 保存JSON文件失败：{e.Message}");
            }
        }

        /// <summary>
        /// 从XML文件加载对话数据库（TODO: 实现XML加载）
        /// </summary>
        public DialogueDatabase LoadFromXml(string xmlPath)
        {
            // TODO: 实现XML加载逻辑
            Debug.LogWarning("[DialogueDatabaseLoader] XML加载功能尚未实现");
            return null;
        }
    }
}

