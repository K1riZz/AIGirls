using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace NewDialogueSystem
{
    /// <summary>
    /// 历史记录对话UI（章节式显示）
    /// </summary>
    public class HistoryDialogueUI : MonoBehaviour, IHistoryDialogueUI
    {
        [Header("UI组件引用")]
        [Tooltip("历史记录面板")]
        public GameObject historyPanel;

        [Tooltip("章节按钮容器（用于放置章节按钮）")]
        public Transform chapterButtonContainer;

        [Tooltip("对话列表容器（ScrollView的Content）")]
        public Transform dialogueListContainer;

        [Tooltip("章节按钮预制体")]
        public GameObject chapterButtonPrefab;

        [Tooltip("历史记录条目预制体")]
        public GameObject historyEntryPrefab;

        [Tooltip("关闭按钮")]
        public Button closeButton;

        [Tooltip("滚动视图")]
        public ScrollRect scrollRect;

        [Header("设置")]
        [Tooltip("最大显示条目数")]
        public int maxDisplayEntries = 100;

        [Tooltip("打开历史UI的快捷键（默认H键）")]
        public KeyCode toggleKey = KeyCode.H;

        private bool isShowing = false;
        private List<GameObject> currentChapterButtons = new List<GameObject>();
        private List<GameObject> currentDialogueEntries = new List<GameObject>();
        private string selectedChapterID = null;

        private void Awake()
        {
            // 自动查找UI组件
            AutoFindUIComponents();

            // 检查并修复缩放问题
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null && (rectTransform.localScale.x == 0 || rectTransform.localScale.y == 0))
            {
                rectTransform.localScale = Vector3.one;
            }

            // 初始化时隐藏
            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }

            // 设置关闭按钮
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideHistory);
            }
        }

        private void Update()
        {
            // 检测快捷键（仅在剧情模式下响应）
            if (Input.GetKeyDown(toggleKey))
            {
                if (isShowing)
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
        /// 自动查找UI组件
        /// </summary>
        private void AutoFindUIComponents()
        {
            if (historyPanel == null)
            {
                historyPanel = FindPanelRecursive(transform);
            }

            // 查找章节按钮容器
            if (chapterButtonContainer == null)
            {
                Transform container = transform.Find("ChapterButtonContainer");
                if (container == null)
                {
                    container = FindChildRecursive(transform, "ChapterButtonContainer");
                }
                if (container != null)
                {
                    chapterButtonContainer = container;
                }
            }

            // 查找对话列表容器
            if (dialogueListContainer == null)
            {
                ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
                if (scrollRect != null && scrollRect.content != null)
                {
                    dialogueListContainer = scrollRect.content;
                    this.scrollRect = scrollRect;
                }
                else
                {
                    Transform container = transform.Find("DialogueListContainer");
                    if (container == null)
                    {
                        container = FindChildRecursive(transform, "DialogueListContainer");
                    }
                    if (container == null)
                    {
                        container = FindChildRecursive(transform, "Content");
                    }
                    if (container != null)
                    {
                        dialogueListContainer = container;
                    }
                }
            }

            if (scrollRect == null)
            {
                scrollRect = GetComponentInChildren<ScrollRect>();
            }

            if (closeButton == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>();
                foreach (var btn in buttons)
                {
                    if (btn.name.Contains("Close") || btn.name.Contains("关闭"))
                    {
                        closeButton = btn;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 递归查找Panel
        /// </summary>
        private GameObject FindPanelRecursive(Transform parent)
        {
            if (parent == null) return null;

            Transform panel = parent.Find("Panel");
            if (panel != null) return panel.gameObject;

            foreach (Transform child in parent)
            {
                GameObject found = FindPanelRecursive(child);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// 递归查找子对象
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child.name == name || child.name.Contains(name)) return child;

                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// 显示历史记录（实现IHistoryDialogueUI接口）
        /// </summary>
        public void ShowHistory()
        {
            isShowing = true;

            // 确保父对象激活
            EnsureParentActive();

            // 显示面板
            if (historyPanel != null)
            {
                historyPanel.SetActive(true);
            }

            // 刷新章节列表和对话列表
            RefreshChapterList();
            
            // 默认选择第一个已解锁的章节
            SelectFirstAvailableChapter();
        }

        /// <summary>
        /// 隐藏历史记录（实现IHistoryDialogueUI接口）
        /// </summary>
        public void HideHistory()
        {
            isShowing = false;

            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 添加历史记录条目（实现IHistoryDialogueUI接口）
        /// </summary>
        public void AddHistoryEntry(DialogueHistoryEntry entry)
        {
            // 如果当前显示的是该章节，刷新对话列表
            if (isShowing && entry.chapterID == selectedChapterID)
            {
                RefreshDialogueList(selectedChapterID);
            }
        }

        /// <summary>
        /// 刷新章节列表（只显示已解锁的章节）
        /// </summary>
        private void RefreshChapterList()
        {
            if (DialogueSystemManager.Instance == null) return;
            if (chapterButtonContainer == null) return;

            // 清除现有章节按钮
            ClearChapterButtons();

            // 获取所有已解锁的章节
            HashSet<string> unlockedChapterIDs = DialogueSystemManager.Instance.GetUnlockedChapters();
            if (unlockedChapterIDs.Count == 0) return;

            // 获取章节数据库
            ChapterDatabase chapterDB = DialogueSystemManager.Instance.chapterDatabase;
            if (chapterDB == null) return;

            // 获取所有章节并按顺序排序
            List<ChapterData> allChapters = chapterDB.GetAllChapters();
            
            // 只显示已解锁的章节
            List<ChapterData> unlockedChapters = allChapters
                .Where(c => unlockedChapterIDs.Contains(c.chapterID))
                .OrderBy(c => c.order)
                .ToList();

            // 如果没有章节数据库，使用历史记录中的章节ID创建临时章节
            if (unlockedChapters.Count == 0)
            {
                foreach (string chapterID in unlockedChapterIDs.OrderBy(id => id))
                {
                    CreateChapterButton(chapterID, chapterID);
                }
            }
            else
            {
                // 创建章节按钮
                foreach (var chapter in unlockedChapters)
                {
                    CreateChapterButton(chapter.chapterID, chapter.chapterName);
                }
            }
        }

        /// <summary>
        /// 创建章节按钮
        /// </summary>
        private void CreateChapterButton(string chapterID, string chapterName)
        {
            GameObject buttonObj = null;

            if (chapterButtonPrefab != null)
            {
                buttonObj = Instantiate(chapterButtonPrefab, chapterButtonContainer);
            }
            else
            {
                buttonObj = CreateDefaultChapterButton();
            }

            if (buttonObj == null) return;

            buttonObj.name = $"ChapterButton_{chapterID}";

            // 设置按钮文本
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                buttonText = buttonObj.GetComponent<TextMeshProUGUI>();
            }

            if (buttonText != null)
            {
                buttonText.text = chapterName;
            }

            // 设置按钮点击事件
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }

            string capturedChapterID = chapterID;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnChapterButtonClicked(capturedChapterID));

            currentChapterButtons.Add(buttonObj);
        }

        /// <summary>
        /// 创建默认章节按钮
        /// </summary>
        private GameObject CreateDefaultChapterButton()
        {
            GameObject buttonObj = new GameObject("ChapterButton");
            buttonObj.transform.SetParent(chapterButtonContainer, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button button = buttonObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "章节";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 18;
            text.color = Color.white;
            text.raycastTarget = false;

            return buttonObj;
        }

        /// <summary>
        /// 章节按钮点击事件
        /// </summary>
        private void OnChapterButtonClicked(string chapterID)
        {
            selectedChapterID = chapterID;
            RefreshDialogueList(chapterID);
        }

        /// <summary>
        /// 选择第一个可用的章节
        /// </summary>
        private void SelectFirstAvailableChapter()
        {
            if (currentChapterButtons.Count > 0)
            {
                Button firstButton = currentChapterButtons[0].GetComponent<Button>();
                if (firstButton != null)
                {
                    firstButton.onClick.Invoke();
                }
            }
        }

        /// <summary>
        /// 刷新对话列表（显示指定章节的对话）
        /// </summary>
        private void RefreshDialogueList(string chapterID)
        {
            if (DialogueSystemManager.Instance == null) return;
            if (dialogueListContainer == null) return;

            // 清除现有对话条目
            ClearDialogueEntries();

            // 获取指定章节的历史记录
            List<DialogueHistoryEntry> chapterHistory = DialogueSystemManager.Instance.GetHistoryByChapter(chapterID);

            // 按时间倒序排列（最新的在前）
            List<DialogueHistoryEntry> sortedHistory = chapterHistory.OrderByDescending(e => e.timestamp).ToList();

            // 限制显示数量
            int displayCount = Mathf.Min(sortedHistory.Count, maxDisplayEntries);

            // 创建对话条目UI
            for (int i = 0; i < displayCount; i++)
            {
                CreateDialogueEntryUI(sortedHistory[i], i);
            }

            // 滚动到顶部（显示最新内容）
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// 创建对话条目UI
        /// </summary>
        private void CreateDialogueEntryUI(DialogueHistoryEntry entry, int index)
        {
            if (dialogueListContainer == null) return;

            GameObject entryObj = null;

            if (historyEntryPrefab != null)
            {
                entryObj = Instantiate(historyEntryPrefab, dialogueListContainer);
            }
            else
            {
                entryObj = CreateDefaultHistoryEntry();
            }

            if (entryObj == null) return;

            entryObj.name = $"DialogueEntry_{entry.timestamp:yyyyMMdd_HHmmss}_{index}";

            // 设置条目内容
            SetupHistoryEntryContent(entryObj, entry);

            // 添加到列表
            currentDialogueEntries.Add(entryObj);
        }

        /// <summary>
        /// 设置历史记录条目内容
        /// </summary>
        private void SetupHistoryEntryContent(GameObject entryObj, DialogueHistoryEntry entry)
        {
            // 查找文本组件
            TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI nameText = texts.Length > 0 ? texts[0] : null;

            if (nameText != null)
            {
                // 格式化显示文本
                string displayText = $"[{entry.timestamp:yyyy-MM-dd HH:mm:ss}] {entry.characterName}: {entry.text}";
                nameText.text = displayText;
            }

            // 显示背景图片
            if (!string.IsNullOrEmpty(entry.backgroundImagePath))
            {
                Image bgImage = entryObj.GetComponentInChildren<Image>();
                if (bgImage == null || bgImage.gameObject.name != "BackgroundImage")
                {
                    // 创建背景图片对象
                    GameObject bgObj = new GameObject("BackgroundImage");
                    bgObj.transform.SetParent(entryObj.transform, false);
                    RectTransform bgRect = bgObj.AddComponent<RectTransform>();
                    bgRect.SetAsFirstSibling(); // 放在最底层
                    bgRect.anchorMin = Vector2.zero;
                    bgRect.anchorMax = Vector2.one;
                    bgRect.sizeDelta = Vector2.zero;
                    bgRect.anchoredPosition = Vector2.zero;
                    bgImage = bgObj.AddComponent<Image>();
                }

                // 加载图片
                Sprite bgSprite = Resources.Load<Sprite>(entry.backgroundImagePath);
                if (bgSprite != null)
                {
                    bgImage.sprite = bgSprite;
                    bgImage.color = Color.white;
                }
            }

            // 显示插入的图片
            if (entry.insertImagePaths != null && entry.insertImagePaths.Count > 0)
            {
                Transform imageContainer = entryObj.transform.Find("ImageContainer");
                if (imageContainer == null)
                {
                    GameObject containerObj = new GameObject("ImageContainer");
                    containerObj.transform.SetParent(entryObj.transform, false);
                    RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                    containerRect.anchorMin = new Vector2(0, 0);
                    containerRect.anchorMax = new Vector2(1, 0.3f);
                    containerRect.sizeDelta = Vector2.zero;
                    containerRect.anchoredPosition = Vector2.zero;
                    imageContainer = containerObj.transform;

                    // 添加HorizontalLayoutGroup
                    HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
                    layoutGroup.spacing = 10f;
                    layoutGroup.childControlWidth = false;
                    layoutGroup.childControlHeight = false;
                }

                foreach (string imagePath in entry.insertImagePaths)
                {
                    if (string.IsNullOrEmpty(imagePath)) continue;

                    GameObject imageObj = new GameObject($"InsertImage_{imagePath}");
                    imageObj.transform.SetParent(imageContainer, false);
                    RectTransform imageRect = imageObj.AddComponent<RectTransform>();
                    imageRect.sizeDelta = new Vector2(200, 150);
                    Image image = imageObj.AddComponent<Image>();

                    Sprite sprite = Resources.Load<Sprite>(imagePath);
                    if (sprite != null)
                    {
                        image.sprite = sprite;
                        image.color = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// 创建默认历史记录条目
        /// </summary>
        private GameObject CreateDefaultHistoryEntry()
        {
            GameObject entryObj = new GameObject("DialogueEntry");
            entryObj.transform.SetParent(dialogueListContainer, false);

            RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 100);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            VerticalLayoutGroup layoutGroup = entryObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            Image image = entryObj.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.3f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(entryObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, 60);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "历史记录条目";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;

            return entryObj;
        }

        /// <summary>
        /// 清除所有章节按钮
        /// </summary>
        private void ClearChapterButtons()
        {
            foreach (GameObject button in currentChapterButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            currentChapterButtons.Clear();
        }

        /// <summary>
        /// 清除所有对话条目
        /// </summary>
        private void ClearDialogueEntries()
        {
            foreach (GameObject entry in currentDialogueEntries)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            currentDialogueEntries.Clear();
        }

        /// <summary>
        /// 确保所有父对象都是激活的
        /// </summary>
        private void EnsureParentActive()
        {
            Transform parent = transform.parent;
            while (parent != null && parent != transform.root)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
