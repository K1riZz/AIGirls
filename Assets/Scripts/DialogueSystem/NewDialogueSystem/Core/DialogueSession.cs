using UnityEngine;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话会话（管理一次完整的对话流程）
    /// 支持多UI、多角色同时对话
    /// </summary>
    public class DialogueSession
    {
        public string sessionID { get; private set; }
        public string startNodeID { get; private set; }
        public DialogueDatabase database { get; private set; }
        public DialogueNode currentNode { get; private set; }
        public bool isActive { get; private set; }

        // 当前使用的UI实例
        private IDialogueUI currentUI;
        private string currentUIInstanceID;
        
        // 目标Transform（用于气泡对话跟随角色）
        private Transform targetTransform;
        
        // 标记当前是否正在显示选择UI
        private bool isShowingChoices = false;

        // 事件
        public System.Action<DialogueNode> OnNodeStarted;
        public System.Action<DialogueNode> OnNodeCompleted;
        public System.Action<DialogueChoice> OnChoiceSelected;
        public System.Action OnSessionEnded;

        public DialogueSession(string sessionID, string startNodeID, DialogueDatabase database)
        {
            this.sessionID = sessionID;
            this.startNodeID = startNodeID;
            this.database = database;
            this.isActive = false;
            this.targetTransform = null;
        }
        
        /// <summary>
        /// 设置目标Transform（气泡UI将跟随此Transform）
        /// </summary>
        public void SetTargetTransform(Transform target)
        {
            this.targetTransform = target;
            
            // 如果当前已经有UI在显示，并且是气泡UI，立即设置跟随目标
            if (currentUI != null && target != null && currentNode != null)
            {
                if (currentNode.displayMode == DialogueDisplayMode.Bubble && currentUI is MonoBehaviour uiBehaviour)
                {
                    var uiType = currentUI.GetType();
                    var setFollowTargetMethod = uiType.GetMethod("SetFollowTarget", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (setFollowTargetMethod != null)
                    {
                        setFollowTargetMethod.Invoke(currentUI, new object[] { target });
                    }
                }
            }
        }

        /// <summary>
        /// 开始对话会话
        /// </summary>
        public void Start()
        {
            if (isActive)
            {
                Debug.LogWarning($"[DialogueSession] 会话 {sessionID} 已经在运行中");
                return;
            }

            isActive = true;
            GotoNode(startNodeID);
        }

        /// <summary>
        /// 结束对话会话
        /// </summary>
        public void End()
        {
            if (!isActive)
                return;

            isActive = false;

            // 隐藏当前UI
            if (currentUI != null)
            {
                currentUI.HideDialogue();
            }

            // 从DialogueSystemManager中移除UI实例
            if (!string.IsNullOrEmpty(currentUIInstanceID) && DialogueSystemManager.Instance != null)
            {
                DialogueSystemManager.Instance.RemoveUIInstance(currentUIInstanceID);
            }

            currentNode = null;
            currentUI = null;
            currentUIInstanceID = null;

            OnSessionEnded?.Invoke();
        }

        /// <summary>
        /// 跳转到指定节点
        /// </summary>
        public void GotoNode(string nodeID)
        {
            if (!isActive)
            {
                Debug.LogWarning("[DialogueSession] 会话未激活");
                return;
            }

            DialogueNode node = database.GetNode(nodeID);
            if (node == null)
            {
                Debug.LogError($"[DialogueSession] 找不到节点：{nodeID}");
                End();
                return;
            }

            // 检查节点是否可以显示
            if (!node.CanDisplay())
            {
                if (!string.IsNullOrEmpty(node.nextNodeID))
                {
                    GotoNode(node.nextNodeID);
                }
                else
                {
                    End();
                }
                return;
            }

            // 完成上一个节点
            if (currentNode != null)
            {
                OnNodeCompleted?.Invoke(currentNode);
            }

            // 设置当前节点
            currentNode = node;

            // 处理节点
            ProcessNode(node);
        }

        /// <summary>
        /// 处理节点
        /// </summary>
        private void ProcessNode(DialogueNode node)
        {
            OnNodeStarted?.Invoke(node);

            switch (node.nodeType)
            {
                case DialogueNodeType.Text:
                    ProcessTextNode(node);
                    break;

                case DialogueNodeType.Choice:
                    ProcessChoiceNode(node);
                    break;

                case DialogueNodeType.Image:
                    ProcessImageNode(node);
                    break;

                case DialogueNodeType.Event:
                    ProcessEventNode(node);
                    break;

                case DialogueNodeType.Jump:
                    ProcessJumpNode(node);
                    break;

                case DialogueNodeType.End:
                    ProcessEndNode(node);
                    break;
            }
        }

        /// <summary>
        /// 处理文本节点
        /// </summary>
        private void ProcessTextNode(DialogueNode node)
        {
            // 获取角色数据
            CharacterData character = null;
            if (!string.IsNullOrEmpty(node.characterID))
            {
                character = database.GetCharacter(node.characterID);
            }

            // 获取或创建UI实例（使用会话ID确保唯一性）
            string baseUIInstanceID = !string.IsNullOrEmpty(node.uiInstanceID) ? node.uiInstanceID : "default";
            string uiInstanceID = $"{baseUIInstanceID}_{sessionID}";
            currentUIInstanceID = uiInstanceID;
            
            currentUI = DialogueSystemManager.Instance.GetOrCreateUI(
                uiInstanceID,
                node.displayMode
            );

            if (currentUI == null)
            {
                Debug.LogError($"[DialogueSession] 无法创建UI实例：{uiInstanceID}");
                End();
                return;
            }

            // 设置UI位置偏移（用于多UI显示）
            if (node.uiOffset != Vector2.zero && currentUI is MonoBehaviour uiMono)
            {
                RectTransform rectTransform = uiMono.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition += node.uiOffset;
                }
            }

            // 如果是气泡对话，设置跟随目标
            if (node.displayMode == DialogueDisplayMode.Bubble && currentUI is MonoBehaviour uiBehaviour)
            {
                var bubbleUI = uiBehaviour.GetComponent<BubbleDialogueUI>();
                if (bubbleUI != null && targetTransform != null)
                {
                    bubbleUI.SetFollowTarget(targetTransform);
                }
            }
            
            // 显示对话
            currentUI.ShowDialogue(node, character);

            // 添加到历史记录（仅在剧情模式下记录）
            if (node.displayMode == DialogueDisplayMode.Story && DialogueSystemManager.Instance != null)
            {
                DialogueHistoryEntry historyEntry = new DialogueHistoryEntry(node, character, sessionID);
                DialogueSystemManager.Instance.AddHistoryEntry(historyEntry);
            }

            // 订阅对话完成事件
            currentUI.OnDialogueCompleted = () =>
            {
                string nextNodeID = GetNextNodeID(node);
                
                if (!string.IsNullOrEmpty(nextNodeID))
                {
                    GotoNode(nextNodeID);
                }
                else
                {
                    End();
                }
            };
        }

        /// <summary>
        /// 处理选择节点
        /// </summary>
        private void ProcessChoiceNode(DialogueNode node)
        {
            isShowingChoices = true;
            
            // 如果当前有StoryDialogueUI显示，暂时禁用它的交互（避免遮挡选择按钮）
            IDialogueUI previousStoryUI = null;
            if (currentUI != null && !(currentUI is IChoiceDialogueUI))
            {
                previousStoryUI = currentUI;
                if (currentUI is MonoBehaviour currentUIMono)
                {
                    CanvasGroup storyGroup = currentUIMono.GetComponent<CanvasGroup>();
                    if (storyGroup != null)
                    {
                        storyGroup.blocksRaycasts = false;
                        storyGroup.interactable = false;
                    }
                    
                    // 禁用StoryDialogueUI脚本的Update点击检测
                    if (currentUI is StoryDialogueUI)
                    {
                        currentUIMono.enabled = false;
                    }
                }
            }
            
            // 获取选择UI
            string choiceUIInstanceID = $"choice_{sessionID}";
            currentUIInstanceID = choiceUIInstanceID;
            
            IDialogueUI choiceUI = DialogueSystemManager.Instance.GetOrCreateUI(
                choiceUIInstanceID,
                DialogueDisplayMode.Custom,
                DialogueSystemManager.Instance.defaultChoiceDialogueUIPrefab
            );

            currentUI = choiceUI;

            if (choiceUI is IChoiceDialogueUI choiceDialogueUI)
            {
                choiceDialogueUI.ShowChoices(
                    node.choices,
                    (choice) =>
                    {
                        // 选择完成后，恢复StoryDialogueUI
                        isShowingChoices = false;
                        if (previousStoryUI is MonoBehaviour prevMono)
                        {
                            CanvasGroup prevGroup = prevMono.GetComponent<CanvasGroup>();
                            if (prevGroup != null)
                            {
                                prevGroup.blocksRaycasts = true;
                                prevGroup.interactable = true;
                            }
                            prevMono.enabled = true;
                        }
                        
                        OnChoiceSelected?.Invoke(choice);
                        
                        // 执行选择效果
                        if (!string.IsNullOrEmpty(choice.effect))
                        {
                            ExecuteEffect(choice.effect);
                        }

                        // 跳转到选择后的节点
                        if (!string.IsNullOrEmpty(choice.nextNodeID))
                        {
                            GotoNode(choice.nextNodeID);
                        }
                        else
                        {
                            End();
                        }
                    }
                );
            }
        }
        
        /// <summary>
        /// 检查是否正在显示选择UI
        /// </summary>
        public bool IsShowingChoices()
        {
            return isShowingChoices;
        }

        /// <summary>
        /// 处理图片节点
        /// </summary>
        private void ProcessImageNode(DialogueNode node)
        {
            ProcessTextNode(node);
        }

        /// <summary>
        /// 处理事件节点
        /// </summary>
        private void ProcessEventNode(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.eventName))
            {
                ExecuteEvent(node.eventName, node.eventData);
            }

            if (!string.IsNullOrEmpty(node.onEnterScript))
            {
                ExecuteScript(node.onEnterScript);
            }

            string nextNodeID = GetNextNodeID(node);
            if (!string.IsNullOrEmpty(nextNodeID))
            {
                GotoNode(nextNodeID);
            }
            else
            {
                End();
            }
        }

        /// <summary>
        /// 处理跳转节点
        /// </summary>
        private void ProcessJumpNode(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.nextNodeID))
            {
                GotoNode(node.nextNodeID);
            }
            else
            {
                End();
            }
        }

        /// <summary>
        /// 处理结束节点
        /// </summary>
        private void ProcessEndNode(DialogueNode node)
        {
            End();
        }

        /// <summary>
        /// 获取下一个节点ID（考虑条件分支）
        /// </summary>
        private string GetNextNodeID(DialogueNode node)
        {
            // 先检查条件分支
            if (node.conditionalBranches != null && node.conditionalBranches.Count > 0)
            {
                var sortedBranches = new List<ConditionalBranch>(node.conditionalBranches);
                sortedBranches.Sort((a, b) => b.priority.CompareTo(a.priority));

                foreach (var branch in sortedBranches)
                {
                    if (EvaluateCondition(branch.condition))
                    {
                        return branch.nextNodeID;
                    }
                }
            }

            return node.nextNodeID;
        }

        /// <summary>
        /// 执行条件评估（TODO: 实现条件系统）
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // TODO: 实现条件表达式评估
            return true;
        }

        /// <summary>
        /// 执行事件
        /// </summary>
        private void ExecuteEvent(string eventName, string eventData)
        {
            // TODO: 实现事件系统
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        private void ExecuteScript(string scriptName)
        {
            // TODO: 实现脚本系统
        }

        /// <summary>
        /// 执行效果
        /// </summary>
        private void ExecuteEffect(string effect)
        {
            // TODO: 实现效果系统
        }
    }
}
