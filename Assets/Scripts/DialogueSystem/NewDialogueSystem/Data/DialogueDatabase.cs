using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话数据库（存储所有对话数据）
    /// </summary>
    [System.Serializable]
    public class DialogueDatabase
    {
        [Header("数据库信息")]
        [Tooltip("数据库ID")]
        public string databaseID;

        [Tooltip("数据库名称")]
        public string databaseName;

        [Tooltip("数据库版本")]
        public string version = "1.0";

        [Header("对话数据")]
        [Tooltip("所有对话节点")]
        public List<DialogueNode> nodes = new List<DialogueNode>();

        [Tooltip("所有角色数据")]
        public List<CharacterData> characters = new List<CharacterData>();

        [Tooltip("对话组（用于组织对话）")]
        public List<DialogueGroup> groups = new List<DialogueGroup>();

        // 内部查找字典（提高查找速度）
        private Dictionary<string, DialogueNode> nodeDict;
        private Dictionary<string, CharacterData> characterDict;

        /// <summary>
        /// 初始化数据库（构建查找字典）
        /// </summary>
        public void Initialize()
        {
            // 构建节点字典
            nodeDict = new Dictionary<string, DialogueNode>();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.nodeID))
                {
                    if (nodeDict.ContainsKey(node.nodeID))
                    {
                        Debug.LogWarning($"对话数据库警告：发现重复的节点ID '{node.nodeID}'");
                    }
                    else
                    {
                        nodeDict[node.nodeID] = node;
                    }
                }
            }

            // 构建角色字典
            characterDict = new Dictionary<string, CharacterData>();
            foreach (var character in characters)
            {
                if (!string.IsNullOrEmpty(character.characterID))
                {
                    if (characterDict.ContainsKey(character.characterID))
                    {
                        Debug.LogWarning($"对话数据库警告：发现重复的角色ID '{character.characterID}'");
                    }
                    else
                    {
                        characterDict[character.characterID] = character;
                    }
                }
            }

            Debug.Log($"对话数据库 '{databaseName}' 初始化完成：{nodes.Count} 个节点，{characters.Count} 个角色");
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        public DialogueNode GetNode(string nodeID)
        {
            if (nodeDict == null)
                Initialize();

            if (string.IsNullOrEmpty(nodeID))
                return null;

            nodeDict.TryGetValue(nodeID, out DialogueNode node);
            return node;
        }

        /// <summary>
        /// 获取角色
        /// </summary>
        public CharacterData GetCharacter(string characterID)
        {
            if (characterDict == null)
                Initialize();

            if (string.IsNullOrEmpty(characterID))
                return null;

            characterDict.TryGetValue(characterID, out CharacterData character);
            return character;
        }

        /// <summary>
        /// 获取所有节点（按ID排序）
        /// </summary>
        public List<DialogueNode> GetAllNodes()
        {
            return nodes.OrderBy(n => n.nodeID).ToList();
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public List<CharacterData> GetAllCharacters()
        {
            return characters;
        }

        /// <summary>
        /// 检查数据库是否有效
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            // 检查节点ID是否唯一
            var nodeIDs = nodes.Where(n => !string.IsNullOrEmpty(n.nodeID)).Select(n => n.nodeID).ToList();
            var duplicateNodes = nodeIDs.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateNodes.Count > 0)
            {
                Debug.LogError($"对话数据库错误：发现重复的节点ID：{string.Join(", ", duplicateNodes)}");
                isValid = false;
            }

            // 检查角色ID是否唯一
            var characterIDs = characters.Where(c => !string.IsNullOrEmpty(c.characterID)).Select(c => c.characterID).ToList();
            var duplicateCharacters = characterIDs.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateCharacters.Count > 0)
            {
                Debug.LogError($"对话数据库错误：发现重复的角色ID：{string.Join(", ", duplicateCharacters)}");
                isValid = false;
            }

            return isValid;
        }
    }

    /// <summary>
    /// 对话组（用于组织对话，如章节、场景等）
    /// </summary>
    [System.Serializable]
    public class DialogueGroup
    {
        [Tooltip("组ID")]
        public string groupID;

        [Tooltip("组名称")]
        public string groupName;

        [Tooltip("组描述")]
        public string description;

        [Tooltip("组中包含的节点ID列表")]
        public List<string> nodeIDs = new List<string>();
    }
}

