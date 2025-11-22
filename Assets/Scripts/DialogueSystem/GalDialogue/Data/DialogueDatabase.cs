using UnityEngine;
using System.Collections.Generic;

namespace GalDialogueSystem
{
    /// <summary>
    /// 对话数据库（包含所有对话数据）
    /// </summary>
    [System.Serializable]
    public class DialogueDatabase
    {
        [Tooltip("数据库ID")]
        public string databaseID;

        [Tooltip("数据库名称")]
        public string databaseName;

        [Tooltip("角色列表")]
        public List<CharacterData> characters = new List<CharacterData>();

        [Tooltip("对话节点字典（key: nodeID, value: DialogueNode）")]
        public Dictionary<string, DialogueNode> nodes = new Dictionary<string, DialogueNode>();

        [Tooltip("对话起点节点ID列表")]
        public List<string> entryNodeIDs = new List<string>();

        /// <summary>
        /// 根据节点ID获取对话节点
        /// </summary>
        public DialogueNode GetNode(string nodeID)
        {
            if (nodes.ContainsKey(nodeID))
                return nodes[nodeID];
            return null;
        }

        /// <summary>
        /// 根据角色ID获取角色数据
        /// </summary>
        public CharacterData GetCharacter(string characterID)
        {
            foreach (var character in characters)
            {
                if (character.characterID == characterID)
                    return character;
            }
            return null;
        }

        /// <summary>
        /// 添加对话节点
        /// </summary>
        public void AddNode(DialogueNode node)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeID))
            {
                nodes[node.nodeID] = node;
            }
        }

        /// <summary>
        /// 检查节点是否存在
        /// </summary>
        public bool HasNode(string nodeID)
        {
            return nodes.ContainsKey(nodeID);
        }
    }
}
