using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace GalDialogueSystem
{
    /// <summary>
    /// 对话历史记录UI
    /// </summary>
    public class HistoryUI : MonoBehaviour
    {
        [Header("UI组件引用")]
        [Tooltip("历史记录面板")]
        public GameObject historyPanel;

        [Tooltip("历史记录滚动视图")]
        public ScrollRect scrollRect;

        [Tooltip("历史记录内容容器")]
        public Transform historyContent;

        [Tooltip("历史记录条目预制体")]
        public GameObject historyEntryPrefab;

        [Tooltip("关闭按钮")]
        public Button closeButton;

        [Tooltip("清空历史按钮")]
        public Button clearButton;

        void Awake()
        {
            // 初始化时隐藏
            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }

            // 绑定按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideHistory);
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearHistory);
            }
        }

        void Update()
        {
            // 按H键显示/隐藏历史记录
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (historyPanel != null && historyPanel.activeSelf)
                {
                    HideHistory();
                }
                else
                {
                    ShowHistory();
                }
            }
        }

        /// <summary>
        /// 显示历史记录
        /// </summary>
        public void ShowHistory()
        {
            if (historyPanel == null)
                return;

            historyPanel.SetActive(true);
            RefreshHistory();
        }

        /// <summary>
        /// 隐藏历史记录
        /// </summary>
        public void HideHistory()
        {
            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新历史记录
        /// </summary>
        private void RefreshHistory()
        {
            if (historyContent == null || historyEntryPrefab == null)
                return;

            // 清除旧的条目
            foreach (Transform child in historyContent)
            {
                Destroy(child.gameObject);
            }

            // 获取历史记录
            if (GalDialogueManager.Instance == null)
                return;

            List<DialogueHistoryEntry> history = GalDialogueManager.Instance.dialogueHistory;

            if (history == null || history.Count == 0)
                return;

            // 创建历史记录条目
            foreach (var entry in history)
            {
                GameObject entryObj = Instantiate(historyEntryPrefab, historyContent);
                HistoryEntryUI entryUI = entryObj.GetComponent<HistoryEntryUI>();

                if (entryUI != null)
                {
                    entryUI.SetHistoryEntry(entry);
                }
            }

            // 滚动到底部
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        private void ClearHistory()
        {
            if (GalDialogueManager.Instance != null)
            {
                GalDialogueManager.Instance.dialogueHistory.Clear();
                RefreshHistory();
            }
        }
    }

    /// <summary>
    /// 历史记录条目UI
    /// </summary>
    public class HistoryEntryUI : MonoBehaviour
    {
        [Header("UI组件")]
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI timeText;

        public void SetHistoryEntry(DialogueHistoryEntry entry)
        {
            if (characterNameText != null)
            {
                characterNameText.text = !string.IsNullOrEmpty(entry.characterName) ? entry.characterName : "未知";
            }

            if (dialogueText != null)
            {
                dialogueText.text = entry.text;
            }

            if (timeText != null)
            {
                timeText.text = entry.timestamp.ToString("HH:mm:ss");
            }
        }
    }
}
