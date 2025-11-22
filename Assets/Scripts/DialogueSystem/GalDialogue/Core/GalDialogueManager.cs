using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace GalDialogueSystem
{
    /// <summary>
    /// GalGame风格对话系统管理器（核心）
    /// </summary>
    public class GalDialogueManager : MonoBehaviour
    {
        public static GalDialogueManager Instance { get; private set; }

        [Header("对话配置")]
        [Tooltip("对话数据库")]
        public DialogueDatabase dialogueDatabase;

        [Tooltip("对话数据库JSON文件路径（相对于Resources文件夹或绝对路径）")]
        public string dialogueDatabasePath;

        [Header("当前对话状态")]
        [Tooltip("当前对话节点ID")]
        public string currentNodeID;

        [Tooltip("当前是否在对话中")]
        public bool isDialogueActive = false;

        [Tooltip("对话历史记录")]
        public List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();

        [Header("事件")]
        public System.Action<DialogueNode> OnDialogueNodeStarted;
        public System.Action<DialogueNode> OnDialogueNodeCompleted;
        public System.Action<string> OnDialogueStarted;
        public System.Action<string> OnDialogueEnded;
        public System.Action<DialogueChoice> OnChoiceSelected;

        private DialogueNode currentNode;
        private Queue<DialogueNode> pendingNodes = new Queue<DialogueNode>();
        private Dictionary<string, DialogueDatabase> loadedDatabases = new Dictionary<string, DialogueDatabase>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // 如果指定了数据库路径，自动加载
            if (!string.IsNullOrEmpty(dialogueDatabasePath))
            {
                LoadDialogueDatabase(dialogueDatabasePath);
            }
            else if (dialogueDatabase == null)
            {
                Debug.LogWarning("[GalDialogueManager] 未指定对话数据库或数据库路径");
            }
        }

        /// <summary>
        /// 加载对话数据库
        /// </summary>
        public void LoadDialogueDatabase(string filePath)
        {
            DialogueDatabase db = DialogueDataLoader.LoadFromJSON(filePath);
            if (db != null)
            {
                dialogueDatabase = db;
                loadedDatabases[db.databaseID] = db;
                Debug.Log($"[GalDialogueManager] 成功加载对话数据库: {db.databaseName}");
            }
        }

        /// <summary>
        /// 开始对话（使用入口节点ID）
        /// </summary>
        public void StartDialogue(string entryNodeID)
        {
            if (dialogueDatabase == null)
            {
                Debug.LogError("[GalDialogueManager] 对话数据库未加载");
                return;
            }

            DialogueNode entryNode = dialogueDatabase.GetNode(entryNodeID);
            if (entryNode == null)
            {
                Debug.LogError($"[GalDialogueManager] 找不到入口节点: {entryNodeID}");
                return;
            }

            StartDialogueNode(entryNode);
        }

        /// <summary>
        /// 开始对话节点
        /// </summary>
        public void StartDialogueNode(DialogueNode node)
        {
            if (node == null)
            {
                Debug.LogError("[GalDialogueManager] 对话节点为空");
                return;
            }

            // 检查节点是否可以显示
            if (!node.CanDisplay())
            {
                Debug.Log($"[GalDialogueManager] 节点 {node.nodeID} 不满足显示条件，跳过");
                // 尝试继续下一个节点
                if (!string.IsNullOrEmpty(node.nextNodeID))
                {
                    DialogueNode nextNode = dialogueDatabase.GetNode(node.nextNodeID);
                    if (nextNode != null)
                    {
                        StartDialogueNode(nextNode);
                    }
                }
                return;
            }

            currentNode = node;
            currentNodeID = node.nodeID;
            isDialogueActive = true;

            // 触发事件
            OnDialogueNodeStarted?.Invoke(node);
            if (!string.IsNullOrEmpty(node.eventName))
            {
                TriggerEvent(node.eventName, node.eventData);
            }

            // 根据节点类型处理
            switch (node.nodeType)
            {
                case DialogueNodeType.Text:
                    // 文本对话由UI系统处理
                    break;
                case DialogueNodeType.Choice:
                    // 选择对话由UI系统处理
                    break;
                case DialogueNodeType.Image:
                    // 图片插入由UI系统处理
                    break;
                case DialogueNodeType.Event:
                    // 事件节点，直接继续
                    CompleteCurrentNode();
                    break;
                case DialogueNodeType.End:
                    // 结束对话
                    EndDialogue();
                    break;
            }
        }

        /// <summary>
        /// 完成当前节点（由UI系统调用）
        /// </summary>
        public void CompleteCurrentNode()
        {
            if (currentNode == null)
                return;

            // 添加到历史记录
            if (currentNode.nodeType == DialogueNodeType.Text || currentNode.nodeType == DialogueNodeType.Choice)
            {
                DialogueHistoryEntry historyEntry = new DialogueHistoryEntry
                {
                    nodeID = currentNode.nodeID,
                    characterID = currentNode.characterID,
                    characterName = currentNode.characterName,
                    text = currentNode.text,
                    timestamp = System.DateTime.Now
                };
                dialogueHistory.Add(historyEntry);
            }

            // 触发事件
            OnDialogueNodeCompleted?.Invoke(currentNode);

            // 处理下一个节点
            if (currentNode.nodeType == DialogueNodeType.Choice)
            {
                // 选择节点需要等待用户选择，不自动继续
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                DialogueNode nextNode = dialogueDatabase.GetNode(currentNode.nextNodeID);
                if (nextNode != null)
                {
                    StartDialogueNode(nextNode);
                }
                else
                {
                    Debug.LogWarning($"[GalDialogueManager] 找不到下一个节点: {currentNode.nextNodeID}");
                    EndDialogue();
                }
            }
            else
            {
                // 没有下一个节点，结束对话
                EndDialogue();
            }
        }

        /// <summary>
        /// 选择选择项
        /// </summary>
        public void SelectChoice(DialogueChoice choice)
        {
            if (choice == null)
                return;

            OnChoiceSelected?.Invoke(choice);

            // 处理选择效果
            if (!string.IsNullOrEmpty(choice.effect))
            {
                // TODO: 处理选择效果（如改变变量、触发事件等）
            }

            // 跳转到下一个节点
            if (!string.IsNullOrEmpty(choice.nextNodeID))
            {
                DialogueNode nextNode = dialogueDatabase.GetNode(choice.nextNodeID);
                if (nextNode != null)
                {
                    StartDialogueNode(nextNode);
                }
                else
                {
                    Debug.LogWarning($"[GalDialogueManager] 找不到选择后的下一个节点: {choice.nextNodeID}");
                    EndDialogue();
                }
            }
            else
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive)
                return;

            string endedNodeID = currentNodeID;
            currentNode = null;
            currentNodeID = null;
            isDialogueActive = false;

            OnDialogueEnded?.Invoke(endedNodeID);
            Debug.Log("[GalDialogueManager] 对话已结束");
        }

        /// <summary>
        /// 获取当前对话节点
        /// </summary>
        public DialogueNode GetCurrentNode()
        {
            return currentNode;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        private void TriggerEvent(string eventName, string eventData)
        {
            // TODO: 实现事件系统
            Debug.Log($"[GalDialogueManager] 触发事件: {eventName}, 数据: {eventData}");
        }

        /// <summary>
        /// 跳转到指定节点
        /// </summary>
        public void JumpToNode(string nodeID)
        {
            if (dialogueDatabase == null)
                return;

            DialogueNode node = dialogueDatabase.GetNode(nodeID);
            if (node != null)
            {
                StartDialogueNode(node);
            }
            else
            {
                Debug.LogError($"[GalDialogueManager] 找不到节点: {nodeID}");
            }
        }
    }

    /// <summary>
    /// 对话历史记录条目
    /// </summary>
    [System.Serializable]
    public class DialogueHistoryEntry
    {
        public string nodeID;
        public string characterID;
        public string characterName;
        public string text;
        public System.DateTime timestamp;
    }
}
