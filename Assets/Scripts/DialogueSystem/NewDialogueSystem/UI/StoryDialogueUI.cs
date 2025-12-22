using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace NewDialogueSystem
{
    /// <summary>
    /// 剧情对话UI（全屏AVG模式）
    /// </summary>
    public class StoryDialogueUI : DialogueUIBase
    {
        [Header("UI组件引用")]
        [Tooltip("对话面板")]
        public GameObject dialoguePanel;

        [Tooltip("角色名称文本")]
        public TextMeshProUGUI characterNameText;

        [Tooltip("对话文本")]
        public TextMeshProUGUI dialogueText;

        [Tooltip("角色立绘/头像")]
        public Image characterPortrait;

        [Tooltip("背景图片")]
        public Image backgroundImage;

        [Tooltip("点击继续提示")]
        public GameObject continueHint;

        [Tooltip("历史对话按钮（点击打开历史对话UI）")]
        public Button historyButton;

        [Tooltip("缩小按钮（点击后切换显示/隐藏剧情对话UI）")]
        public Button zoomoutButton;

        [Header("动画设置")]
        [Tooltip("缩小动画时长（秒）")]
        public float minimizeAnimationDuration = 0.3f;

        [Tooltip("缩小后的目标位置（相对于屏幕右下角的偏移）")]
        public Vector2 minimizedPosition = new Vector2(-50, 50);

        [Tooltip("缩小后的目标缩放")]
        public Vector2 minimizedScale = new Vector2(0.1f, 0.1f);

        private Coroutine autoAdvanceCoroutine;
        private bool isMinimized = false; // 标记对话UI是否已缩小
        private RectTransform dialoguePanelRect; // 对话面板的RectTransform
        private Vector2 originalPosition; // 原始位置
        private Vector2 originalScale; // 原始缩放

        public override bool IsShowing => isShowing;
        public override DialogueDisplayMode DisplayMode => DialogueDisplayMode.Story;

        protected override void Awake()
        {
            base.Awake();

            // 自动查找UI组件
            AutoFindUIComponents();

            // 检查并修复缩放问题
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null && (rectTransform.localScale.x == 0 || rectTransform.localScale.y == 0))
            {
                rectTransform.localScale = Vector3.one;
            }

            // 初始化时隐藏
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // 设置历史按钮事件
            if (historyButton != null)
            {
                historyButton.onClick.AddListener(OnHistoryButtonClicked);
            }

            // 设置缩小按钮事件
            if (zoomoutButton != null)
            {
                zoomoutButton.onClick.AddListener(OnZoomoutButtonClicked);
            }

            // 获取对话面板的RectTransform并保存原始状态
            if (dialoguePanel != null)
            {
                dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
                if (dialoguePanelRect != null)
                {
                    originalPosition = dialoguePanelRect.anchoredPosition;
                    originalScale = dialoguePanelRect.localScale;

                    // 将按钮移到与DialoguePanel同级（如果它们在DialoguePanel下）
                    Transform panelParent = dialoguePanelRect.parent;
                    if (panelParent != null)
                    {
                        if (historyButton != null)
                        {
                            RectTransform historyRect = historyButton.GetComponent<RectTransform>();
                            if (historyRect != null && historyRect.parent == dialoguePanelRect)
                            {
                                // 按钮在DialoguePanel下，移到同级（保持世界空间位置）
                                historyRect.SetParent(panelParent, true);
                            }
                        }

                        if (zoomoutButton != null)
                        {
                            RectTransform zoomoutRect = zoomoutButton.GetComponent<RectTransform>();
                            if (zoomoutRect != null && zoomoutRect.parent == dialoguePanelRect)
                            {
                                // 按钮在DialoguePanel下，移到同级（保持世界空间位置）
                                zoomoutRect.SetParent(panelParent, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 自动查找UI组件
        /// </summary>
        private void AutoFindUIComponents()
        {
            if (dialoguePanel == null)
            {
                dialoguePanel = FindPanelRecursive(transform);
            }

            if (dialoguePanel != null)
            {
                if (characterNameText == null)
                {
                    characterNameText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
                }

                if (dialogueText == null)
                {
                    TextMeshProUGUI[] texts = dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length > 1)
                    {
                        dialogueText = texts[1];
                    }
                    else if (texts.Length > 0)
                    {
                        dialogueText = texts[0];
                    }
                }

                if (characterPortrait == null)
                {
                    Image[] images = dialoguePanel.GetComponentsInChildren<Image>();
                    foreach (var img in images)
                    {
                        if (img.name.Contains("Portrait") || img.name.Contains("Character"))
                        {
                            characterPortrait = img;
                            break;
                        }
                    }
                }

                if (backgroundImage == null)
                {
                    Image[] images = dialoguePanel.GetComponentsInChildren<Image>();
                    foreach (var img in images)
                    {
                        if (img.name.Contains("Background") || img.name.Contains("BG"))
                        {
                            backgroundImage = img;
                            break;
                        }
                    }
                }
            }

            // 自动查找历史按钮
            if (historyButton == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>();
                foreach (var btn in buttons)
                {
                    if (btn.name.Contains("History") || btn.name.Contains("历史"))
                    {
                        historyButton = btn;
                        break;
                    }
                }
            }

            // 自动查找缩小按钮
            if (zoomoutButton == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>();
                foreach (var btn in buttons)
                {
                    if (btn.name.Contains("Zoomout") || btn.name.Contains("ZoomoutButton"))
                    {
                        zoomoutButton = btn;
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

        public override void ShowDialogue(DialogueNode node, CharacterData character)
        {
            if (node == null) return;

            currentNode = node;
            currentCharacter = character;

            // 停止之前的协程
            StopAllCoroutines();

            // 确保父对象激活
            EnsureParentActive();

            // 显示面板并恢复原始状态
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
                isMinimized = false;
                
                // 恢复原始位置和缩放
                if (dialoguePanelRect != null)
                {
                    dialoguePanelRect.anchoredPosition = originalPosition;
                    dialoguePanelRect.localScale = originalScale;
                }
            }

            // 确保缩小按钮显示
            if (zoomoutButton != null)
            {
                zoomoutButton.gameObject.SetActive(true);
            }

            // 设置角色名称
            if (characterNameText != null)
            {
                characterNameText.text = node.GetCharacterName(character);
                if (character != null)
                {
                    characterNameText.color = character.nameColor;
                }
            }

            // 设置角色立绘
            if (characterPortrait != null && character != null)
            {
                Sprite portrait = character.GetPortraitSprite();
                if (portrait != null)
                {
                    characterPortrait.sprite = portrait;
                    characterPortrait.gameObject.SetActive(true);
                }
                else
                {
                    characterPortrait.gameObject.SetActive(false);
                }
            }

            // 设置背景图片
            if (backgroundImage != null && !string.IsNullOrEmpty(node.backgroundImagePath))
            {
                Sprite bg = Resources.Load<Sprite>(node.backgroundImagePath);
                if (bg != null)
                {
                    backgroundImage.sprite = bg;
                    backgroundImage.gameObject.SetActive(true);
                }
            }

            // 设置文本颜色
            if (dialogueText != null && character != null)
            {
                dialogueText.color = character.textColor;
            }

            // 开始打字动画
            float textSpeed = node.textSpeed > 0 ? node.textSpeed : defaultTextSpeed;
            if (textSpeed <= 0) textSpeed = 9999f;

            textTypingCoroutine = StartCoroutine(TypeTextCoroutine(dialogueText, node.text, textSpeed));

            // 设置自动前进
            float autoAdvance = node.autoAdvanceTime > 0 ? node.autoAdvanceTime : 0f;
            if (autoAdvance > 0)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine(autoAdvance));
            }

            isShowing = true;

            // 淡入动画
            StartCoroutine(FadeInCoroutine());
        }

        public override void HideDialogue()
        {
            isShowing = false;
            StopAllCoroutines();

            StartCoroutine(HideDialogueCoroutine());
        }

        /// <summary>
        /// 隐藏对话协程（淡出后立即隐藏面板）
        /// </summary>
        private IEnumerator HideDialogueCoroutine()
        {
            // 执行淡出动画
            yield return StartCoroutine(FadeOutCoroutine());
            
            // 淡出完成后立即隐藏面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // 隐藏缩小按钮
            if (zoomoutButton != null)
            {
                zoomoutButton.gameObject.SetActive(false);
            }
        }

        private IEnumerator AutoAdvanceCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            autoAdvanceCoroutine = null;
            
            while (textTypingCoroutine != null)
            {
                yield return null;
            }

            CompleteDialogue();
        }

        protected override void OnTypingComplete()
        {
            base.OnTypingComplete();

            // 显示继续提示
            if (continueHint != null && (currentNode == null || currentNode.autoAdvanceTime <= 0))
            {
                continueHint.SetActive(true);
            }
        }

        void Update()
        {
            // 如果CanvasGroup的blocksRaycasts为false，说明正在显示选择UI，不处理点击
            if (canvasGroup != null && !canvasGroup.blocksRaycasts)
            {
                return;
            }
            
            // 检测点击继续
            if (isShowing && Input.GetMouseButtonDown(0))
            {
                // 检查是否点击了历史按钮（避免点击按钮时触发对话继续）
                if (historyButton != null && historyButton.gameObject.activeInHierarchy)
                {
                    RectTransform buttonRect = historyButton.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            buttonRect,
                            Input.mousePosition,
                            null,
                            out localPoint
                        );
                        
                        if (buttonRect.rect.Contains(localPoint))
                        {
                            // 点击了历史按钮，不处理对话继续
                            return;
                        }
                    }
                }

                // 检查点击是否在UI区域内（避免点击其他区域触发）
                if (dialoguePanel != null)
                {
                    RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
                    if (panelRect != null)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            panelRect,
                            Input.mousePosition,
                            null,
                            out localPoint
                        );
                        
                        if (!panelRect.rect.Contains(localPoint))
                        {
                            return;
                        }
                    }
                }
                
                if (textTypingCoroutine != null)
                {
                    // 正在打字，立即完成
                    StopCoroutine(textTypingCoroutine);
                    textTypingCoroutine = null;
                    if (dialogueText != null && currentNode != null)
                    {
                        dialogueText.text = currentNode.text;
                    }
                    OnTypingComplete();
                }
                else
                {
                    // 已完成打字，继续下一个
                    CompleteDialogue();
                }
            }
        }

        private void CompleteDialogue()
        {
            if (continueHint != null)
            {
                continueHint.SetActive(false);
            }
            OnDialogueCompleted?.Invoke();
        }

        /// <summary>
        /// 历史按钮点击事件
        /// </summary>
        private void OnHistoryButtonClicked()
        {
            OpenHistoryUI();
        }

        /// <summary>
        /// 缩小按钮点击事件（切换显示/隐藏）
        /// </summary>
        private void OnZoomoutButtonClicked()
        {
            if (isMinimized)
            {
                // 当前已缩小，恢复显示
                RestoreDialoguePanel();
            }
            else
            {
                // 当前显示中，缩小隐藏
                MinimizeDialoguePanel();
            }
        }

        /// <summary>
        /// 缩小对话面板（带动画）
        /// </summary>
        private void MinimizeDialoguePanel()
        {
            if (dialoguePanel == null || dialoguePanelRect == null) return;

            // 隐藏选项UI
            ChoiceDialogueUI choiceUI = FindObjectOfType<ChoiceDialogueUI>();
            if (choiceUI != null && choiceUI.choicePanel != null)
            {
                choiceUI.choicePanel.SetActive(false);
            }

            // 开始缩小动画
            StartCoroutine(MinimizeAnimationCoroutine());
        }

        /// <summary>
        /// 恢复对话面板（带动画）
        /// </summary>
        private void RestoreDialoguePanel()
        {
            if (dialoguePanel == null || dialoguePanelRect == null) return;

            // 显示面板
            dialoguePanel.SetActive(true);

            // 开始恢复动画
            StartCoroutine(RestoreAnimationCoroutine());
        }

        /// <summary>
        /// 缩小动画协程
        /// </summary>
        private IEnumerator MinimizeAnimationCoroutine()
        {
            if (dialoguePanelRect == null) yield break;

            Vector2 startPosition = dialoguePanelRect.anchoredPosition;
            Vector2 startScale = dialoguePanelRect.localScale;

            // 计算目标位置（屏幕右下角）
            Canvas canvas = GetComponentInParent<Canvas>();
            Vector2 targetPosition = minimizedPosition;
            
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    float canvasWidth = canvasRect.rect.width;
                    float canvasHeight = canvasRect.rect.height;
                    targetPosition = new Vector2(
                        canvasWidth / 2 - minimizedPosition.x,
                        -canvasHeight / 2 + minimizedPosition.y
                    );
                }
            }

            Vector2 targetScale = minimizedScale;
            float timer = 0f;

            while (timer < minimizeAnimationDuration)
            {
                timer += Time.deltaTime;
                float t = timer / minimizeAnimationDuration;
                t = t * t * (3f - 2f * t); // 缓动函数（easeInOut）

                dialoguePanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                dialoguePanelRect.localScale = Vector2.Lerp(startScale, targetScale, t);

                yield return null;
            }

            // 动画完成后隐藏面板
            dialoguePanelRect.anchoredPosition = targetPosition;
            dialoguePanelRect.localScale = targetScale;
            dialoguePanel.SetActive(false);
            isMinimized = true;
        }

        /// <summary>
        /// 恢复动画协程
        /// </summary>
        private IEnumerator RestoreAnimationCoroutine()
        {
            if (dialoguePanelRect == null) yield break;

            Vector2 startPosition = dialoguePanelRect.anchoredPosition;
            Vector2 startScale = dialoguePanelRect.localScale;
            Vector2 targetPosition = originalPosition;
            Vector2 targetScale = originalScale;

            float timer = 0f;

            while (timer < minimizeAnimationDuration)
            {
                timer += Time.deltaTime;
                float t = timer / minimizeAnimationDuration;
                t = t * t * (3f - 2f * t); // 缓动函数（easeInOut）

                dialoguePanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                dialoguePanelRect.localScale = Vector2.Lerp(startScale, targetScale, t);

                yield return null;
            }

            // 动画完成后恢复原始状态
            dialoguePanelRect.anchoredPosition = targetPosition;
            dialoguePanelRect.localScale = targetScale;
            isMinimized = false;
        }

        /// <summary>
        /// 打开历史对话UI
        /// </summary>
        private void OpenHistoryUI()
        {
            if (DialogueSystemManager.Instance == null) return;

            // 隐藏剧情对话UI
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // 获取历史UI预制体
            GameObject historyUIPrefab = DialogueSystemManager.Instance.defaultHistoryDialogueUIPrefab;
            if (historyUIPrefab == null)
            {
                Debug.LogWarning("[StoryDialogueUI] 历史对话UI预制体未配置");
                return;
            }

            // 查找现有的历史UI实例
            HistoryDialogueUI existingHistoryUI = FindObjectOfType<HistoryDialogueUI>();
            
            if (existingHistoryUI != null)
            {
                // 如果已存在，直接显示
                existingHistoryUI.ShowHistory();
            }
            else
            {
                // 如果不存在，创建新实例
                Transform container = DialogueSystemManager.Instance.dialogueUIContainer;
                if (container == null)
                {
                    container = DialogueSystemManager.Instance.transform;
                }

                GameObject historyUIObj = Instantiate(historyUIPrefab, container);
                historyUIObj.name = "HistoryDialogueUI";

                HistoryDialogueUI historyUI = historyUIObj.GetComponent<HistoryDialogueUI>();
                if (historyUI != null)
                {
                    historyUI.ShowHistory();
                }
                else
                {
                    Debug.LogError("[StoryDialogueUI] 历史对话UI预制体缺少HistoryDialogueUI组件");
                }
            }
        }
    }
}
