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

        private Coroutine autoAdvanceCoroutine;

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

            // 显示面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
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

            StartCoroutine(FadeOutCoroutine());
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
    }
}
