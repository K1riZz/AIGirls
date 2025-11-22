using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace GalDialogueSystem
{
    /// <summary>
    /// 对话数据加载器（支持JSON和XML格式）
    /// </summary>
    public static class DialogueDataLoader
    {
        /// <summary>
        /// 从JSON文件加载对话数据库
        /// </summary>
        public static DialogueDatabase LoadFromJSON(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("[DialogueDataLoader] 文件路径为空");
                return null;
            }

            string jsonContent = null;

            // 尝试从Resources加载
            if (!filePath.StartsWith("/") && !filePath.Contains(":"))
            {
                TextAsset textAsset = Resources.Load<TextAsset>(filePath);
                if (textAsset != null)
                {
                    jsonContent = textAsset.text;
                }
            }

            // 如果Resources加载失败，尝试从文件系统加载
            if (string.IsNullOrEmpty(jsonContent))
            {
                if (File.Exists(filePath))
                {
                    jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                }
                else
                {
                    Debug.LogError($"[DialogueDataLoader] 文件不存在: {filePath}");
                    return null;
                }
            }

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError($"[DialogueDataLoader] 无法读取文件内容: {filePath}");
                return null;
            }

            try
            {
                // 解析JSON
                DialogueDatabaseJSON jsonData = JsonUtility.FromJson<DialogueDatabaseJSON>(jsonContent);
                
                // 转换为DialogueDatabase
                DialogueDatabase database = new DialogueDatabase
                {
                    databaseID = jsonData.databaseID,
                    databaseName = jsonData.databaseName,
                    entryNodeIDs = jsonData.entryNodeIDs ?? new List<string>()
                };

                // 加载角色数据
                if (jsonData.characters != null)
                {
                    database.characters = jsonData.characters;
                }

                // 加载对话节点
                if (jsonData.nodes != null)
                {
                    foreach (var nodeJSON in jsonData.nodes)
                    {
                        DialogueNode node = ConvertFromJSON(nodeJSON);
                        if (node != null)
                        {
                            database.AddNode(node);
                        }
                    }
                }

                Debug.Log($"[DialogueDataLoader] 成功加载对话数据库: {database.databaseName}，包含 {database.nodes.Count} 个节点");
                return database;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueDataLoader] 解析JSON失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从XML文件加载对话数据库
        /// </summary>
        public static DialogueDatabase LoadFromXML(string filePath)
        {
            // TODO: 实现XML加载逻辑
            Debug.LogWarning("[DialogueDataLoader] XML加载功能尚未实现");
            return null;
        }

        /// <summary>
        /// 将对话数据库保存为JSON文件
        /// </summary>
        public static void SaveToJSON(DialogueDatabase database, string filePath)
        {
            if (database == null)
            {
                Debug.LogError("[DialogueDataLoader] 对话数据库为空");
                return;
            }

            try
            {
                DialogueDatabaseJSON jsonData = new DialogueDatabaseJSON
                {
                    databaseID = database.databaseID,
                    databaseName = database.databaseName,
                    entryNodeIDs = database.entryNodeIDs,
                    characters = database.characters
                };

                // 转换节点列表
                jsonData.nodes = new List<DialogueNodeJSON>();
                foreach (var node in database.nodes.Values)
                {
                    jsonData.nodes.Add(ConvertToJSON(node));
                }

                string jsonContent = JsonUtility.ToJson(jsonData, true);
                File.WriteAllText(filePath, jsonContent, Encoding.UTF8);
                Debug.Log($"[DialogueDataLoader] 成功保存对话数据库到: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueDataLoader] 保存JSON失败: {e.Message}");
            }
        }

        /// <summary>
        /// 将DialogueNode转换为JSON格式
        /// </summary>
        private static DialogueNodeJSON ConvertToJSON(DialogueNode node)
        {
            return new DialogueNodeJSON
            {
                nodeID = node.nodeID,
                nodeType = node.nodeType.ToString(),
                dialogueMode = node.dialogueMode.ToString(),
                characterID = node.characterID,
                text = node.text,
                characterName = node.characterName,
                portraitSpritePath = node.portraitSpritePath,
                backgroundImagePath = node.backgroundImagePath,
                insertImagePath = node.insertImagePath,
                choices = node.choices ?? new List<DialogueChoice>(),
                nextNodeID = node.nextNodeID,
                eventName = node.eventName,
                eventData = node.eventData,
                condition = node.condition,
                textSpeed = node.textSpeed,
                autoAdvanceTime = node.autoAdvanceTime,
                soundEffectPath = node.soundEffectPath,
                backgroundMusicPath = node.backgroundMusicPath
            };
        }

        /// <summary>
        /// 从JSON格式创建DialogueNode
        /// </summary>
        private static DialogueNode ConvertFromJSON(DialogueNodeJSON json)
        {
            DialogueNode node = new DialogueNode
            {
                nodeID = json.nodeID,
                characterID = json.characterID,
                text = json.text,
                characterName = json.characterName,
                portraitSpritePath = json.portraitSpritePath,
                backgroundImagePath = json.backgroundImagePath,
                insertImagePath = json.insertImagePath,
                choices = json.choices ?? new List<DialogueChoice>(),
                nextNodeID = json.nextNodeID,
                eventName = json.eventName,
                eventData = json.eventData,
                condition = json.condition,
                textSpeed = json.textSpeed,
                autoAdvanceTime = json.autoAdvanceTime,
                soundEffectPath = json.soundEffectPath,
                backgroundMusicPath = json.backgroundMusicPath
            };

            // 解析枚举类型
            if (System.Enum.TryParse<DialogueNodeType>(json.nodeType, out DialogueNodeType nodeType))
            {
                node.nodeType = nodeType;
            }

            if (System.Enum.TryParse<DialogueMode>(json.dialogueMode, out DialogueMode dialogueMode))
            {
                node.dialogueMode = dialogueMode;
            }

            return node;
        }

        /// <summary>
        /// JSON序列化用的数据结构
        /// </summary>
        [System.Serializable]
        private class DialogueDatabaseJSON
        {
            public string databaseID;
            public string databaseName;
            public List<CharacterData> characters;
            public List<DialogueNodeJSON> nodes;
            public List<string> entryNodeIDs;
        }

        /// <summary>
        /// JSON序列化用的节点数据结构
        /// </summary>
        [System.Serializable]
        private class DialogueNodeJSON
        {
            public string nodeID;
            public string nodeType;
            public string dialogueMode;
            public string characterID;
            public string text;
            public string characterName;
            public string portraitSpritePath;
            public string backgroundImagePath;
            public string insertImagePath;
            public List<DialogueChoice> choices;
            public string nextNodeID;
            public string eventName;
            public string eventData;
            public string condition;
            public float textSpeed;
            public float autoAdvanceTime;
            public string soundEffectPath;
            public string backgroundMusicPath;
        }
    }
}
