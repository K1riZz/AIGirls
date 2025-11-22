using UnityEngine;
using System.Collections.Generic;

namespace GalDialogueSystem
{
    /// <summary>
    /// 对话UI管理器（管理多种对话UI）
    /// </summary>
    public class DialogueUIManager : MonoBehaviour
    {
        public static DialogueUIManager Instance { get; private set; }

        [Header("对话UI预制体")]
        [Tooltip("剧情对话UI预制体")]
        public GameObject storyDialogueUIPrefab;

        [Tooltip("气泡对话UI预制体")]
        public GameObject bubbleDialogueUIPrefab;

        [Tooltip("选择对话UI预制体")]
        public GameObject choiceDialogueUIPrefab;

        [Tooltip("历史记录UI预制体")]
        public GameObject historyUIPrefab;

        [Header("当前对话UI")]
        private Dictionary<DialogueMode, IDialogueUI> dialogueUIs = new Dictionary<DialogueMode, IDialogueUI>();
        private IDialogueUI currentDialogueUI;
        private ChoiceDialogueUI choiceDialogueUI;
        private HistoryUI historyUI;

        private Transform uiContainer;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 查找或创建UI容器
            GameObject containerObj = GameObject.Find("DialogueUIContainer");
            if (containerObj == null)
            {
                containerObj = new GameObject("DialogueUIContainer");
            }
            uiContainer = containerObj.transform;

            // 初始化UI
            InitializeUIs();
        }

        void Start()
        {
            // 订阅对话管理器事件
            if (GalDialogueManager.Instance != null)
            {
                GalDialogueManager.Instance.OnDialogueNodeStarted += OnDialogueNodeStarted;
                GalDialogueManager.Instance.OnDialogueNodeCompleted += OnDialogueNodeCompleted;
                GalDialogueManager.Instance.OnChoiceSelected += OnChoiceSelected;
                GalDialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
            }
        }

        void OnDestroy()
        {
            // 取消订阅
            if (GalDialogueManager.Instance != null)
            {
                GalDialogueManager.Instance.OnDialogueNodeStarted -= OnDialogueNodeStarted;
                GalDialogueManager.Instance.OnDialogueNodeCompleted -= OnDialogueNodeCompleted;
                GalDialogueManager.Instance.OnChoiceSelected -= OnChoiceSelected;
                GalDialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUIs()
        {
            // 创建剧情对话UI
            if (storyDialogueUIPrefab != null)
            {
                GameObject storyUI = Instantiate(storyDialogueUIPrefab, uiContainer);
                IDialogueUI storyDialogueUI = storyUI.GetComponent<IDialogueUI>();
                if (storyDialogueUI != null)
                {
                    dialogueUIs[DialogueMode.Story] = storyDialogueUI;
                    storyDialogueUI.OnDialogueCompleted += OnDialogueCompleted;
                }
            }

            // 创建气泡对话UI
            if (bubbleDialogueUIPrefab != null)
            {
                GameObject bubbleUI = Instantiate(bubbleDialogueUIPrefab, uiContainer);
                IDialogueUI bubbleDialogueUI = bubbleUI.GetComponent<IDialogueUI>();
                if (bubbleDialogueUI != null)
                {
                    dialogueUIs[DialogueMode.Bubble] = bubbleDialogueUI;
                    bubbleDialogueUI.OnDialogueCompleted += OnDialogueCompleted;
                }
            }

            // 创建选择对话UI
            if (choiceDialogueUIPrefab != null)
            {
                GameObject choiceUI = Instantiate(choiceDialogueUIPrefab, uiContainer);
                choiceDialogueUI = choiceUI.GetComponent<ChoiceDialogueUI>();
            }

            // 创建历史记录UI
            if (historyUIPrefab != null)
            {
                GameObject historyUIObj = Instantiate(historyUIPrefab, uiContainer);
                historyUI = historyUIObj.GetComponent<HistoryUI>();
            }
        }

        /// <summary>
        /// 对话节点开始事件
        /// </summary>
        private void OnDialogueNodeStarted(DialogueNode node)
        {
            if (node == null)
                return;

            // 隐藏当前UI
            if (currentDialogueUI != null && currentDialogueUI.IsShowing)
            {
                currentDialogueUI.HideDialogue();
            }

            // 根据节点类型和模式显示UI
            switch (node.nodeType)
            {
                case DialogueNodeType.Choice:
                    // 显示选择UI
                    if (choiceDialogueUI != null && node.choices != null)
                    {
                        choiceDialogueUI.ShowChoices(node.choices, OnChoiceSelected);
                    }
                    break;

                case DialogueNodeType.Text:
                case DialogueNodeType.Image:
                    // 显示对话UI
                    ShowDialogueUI(node);
                    break;
            }
        }

        /// <summary>
        /// 显示对话UI
        /// </summary>
        private void ShowDialogueUI(DialogueNode node)
        {
            // 获取对应的UI
            if (!dialogueUIs.ContainsKey(node.dialogueMode))
            {
                Debug.LogWarning($"[DialogueUIManager] 找不到对话模式 {node.dialogueMode} 对应的UI");
                return;
            }

            currentDialogueUI = dialogueUIs[node.dialogueMode];

            // 获取角色数据
            CharacterData character = null;
            if (GalDialogueManager.Instance != null && GalDialogueManager.Instance.dialogueDatabase != null)
            {
                if (!string.IsNullOrEmpty(node.characterID))
                {
                    character = GalDialogueManager.Instance.dialogueDatabase.GetCharacter(node.characterID);
                }
            }

            // 显示对话
            currentDialogueUI.ShowDialogue(node, character);
        }

        /// <summary>
        /// 对话完成事件
        /// </summary>
        private void OnDialogueCompleted()
        {
            // 通知对话管理器继续
            if (GalDialogueManager.Instance != null)
            {
                GalDialogueManager.Instance.CompleteCurrentNode();
            }
        }

        /// <summary>
        /// 对话节点完成事件
        /// </summary>
        private void OnDialogueNodeCompleted(DialogueNode node)
        {
            // 隐藏当前UI
            if (currentDialogueUI != null && currentDialogueUI.IsShowing)
            {
                currentDialogueUI.HideDialogue();
            }
            currentDialogueUI = null;
        }

        /// <summary>
        /// 选择项选择事件
        /// </summary>
        private void OnChoiceSelected(DialogueChoice choice)
        {
            if (GalDialogueManager.Instance != null)
            {
                GalDialogueManager.Instance.SelectChoice(choice);
            }
        }

        /// <summary>
        /// 对话结束事件
        /// </summary>
        private void OnDialogueEnded(string nodeID)
        {
            // 隐藏所有UI
            if (currentDialogueUI != null && currentDialogueUI.IsShowing)
            {
                currentDialogueUI.HideDialogue();
            }
            currentDialogueUI = null;

            if (choiceDialogueUI != null)
            {
                choiceDialogueUI.HideChoices();
            }
        }

        /// <summary>
        /// 显示历史记录
        /// </summary>
        public void ShowHistory()
        {
            if (historyUI != null)
            {
                historyUI.ShowHistory();
            }
        }

        /// <summary>
        /// 隐藏历史记录
        /// </summary>
        public void HideHistory()
        {
            if (historyUI != null)
            {
                historyUI.HideHistory();
            }
        }
    }
}
