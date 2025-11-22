using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GalDialogueSystem
{
    /// <summary>
    /// 剧情对话UI（全屏AVG模式）
    /// </summary>
    public class StoryDialogueUI : MonoBehaviour, IDialogueUI
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

        [Tooltip("插入的图片（可选）")]
        public Image insertImage;

        [Tooltip("点击继续提示")]
        public GameObject continueHint;

        [Tooltip("自动前进进度条（可选）")]
        public Slider autoAdvanceSlider;

        [Header("动画设置")]
        [Tooltip("淡入淡出时间")]
        public float fadeDuration = 0.3f;

        [Tooltip("文字显示动画速度（字符/秒）")]
        public float defaultTextSpeed = 30f;

        private DialogueMode dialogueMode = DialogueMode.Story;
        private bool isShowing = false;
        private DialogueNode currentNode;
        private CharacterData currentCharacter;
        private Coroutine textTypingCoroutine;
        private Coroutine autoAdvanceCoroutine;
        private CanvasGroup canvasGroup;

        public bool IsShowing => isShowing;
        public DialogueMode DialogueMode => dialogueMode;
        public System.Action OnDialogueCompleted { get; set; }

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 初始化时隐藏
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        void Update()
        {
            // 检测点击继续
            if (isShowing && Input.GetMouseButtonDown(0))
            {
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

        public void ShowDialogue(DialogueNode node, CharacterData character)
        {
            if (node == null)
                return;

            currentNode = node;
            currentCharacter = character;

            // 停止之前的协程
            if (textTypingCoroutine != null)
            {
                StopCoroutine(textTypingCoroutine);
                textTypingCoroutine = null;
            }
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }

            // 显示UI
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // 设置角色名称
            if (characterNameText != null)
            {
                string displayName = !string.IsNullOrEmpty(node.characterName) ? node.characterName :
                                   (character != null ? character.characterName : "未知");
                characterNameText.text = displayName;

                // 设置名称颜色
                if (character != null)
                {
                    characterNameText.color = character.nameColor;
                }
            }

            // 设置角色立绘
            if (characterPortrait != null)
            {
                Sprite portrait = null;
                if (!string.IsNullOrEmpty(node.portraitSpritePath))
                {
                    portrait = Resources.Load<Sprite>(node.portraitSpritePath);
                }
                else if (character != null)
                {
                    portrait = character.GetPortraitSprite();
                }

                characterPortrait.sprite = portrait;
                characterPortrait.gameObject.SetActive(portrait != null);
            }

            // 设置背景图片
            if (backgroundImage != null && !string.IsNullOrEmpty(node.backgroundImagePath))
            {
                Sprite bg = Resources.Load<Sprite>(node.backgroundImagePath);
                backgroundImage.sprite = bg;
                backgroundImage.gameObject.SetActive(bg != null);
            }

            // 设置插入图片
            if (insertImage != null && !string.IsNullOrEmpty(node.insertImagePath))
            {
                Sprite insert = Resources.Load<Sprite>(node.insertImagePath);
                insertImage.sprite = insert;
                insertImage.gameObject.SetActive(insert != null);
            }

            // 设置文本颜色
            if (dialogueText != null && character != null)
            {
                dialogueText.color = character.textColor;
            }

            // 开始打字动画
            float textSpeed = node.textSpeed > 0 ? node.textSpeed : defaultTextSpeed;
            textTypingCoroutine = StartCoroutine(TypeText(node.text, textSpeed));

            // 设置自动前进
            if (node.autoAdvanceTime > 0)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvance(node.autoAdvanceTime));
            }

            isShowing = true;

            // 淡入动画
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        public void HideDialogue()
        {
            isShowing = false;

            // 停止协程
            if (textTypingCoroutine != null)
            {
                StopCoroutine(textTypingCoroutine);
                textTypingCoroutine = null;
            }
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }

            // 淡出动画
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private IEnumerator TypeText(string text, float speed)
        {
            if (dialogueText == null)
                yield break;

            dialogueText.text = "";
            float charDelay = 1f / speed;

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            OnTypingComplete();
        }

        private void OnTypingComplete()
        {
            textTypingCoroutine = null;

            // 显示继续提示
            if (continueHint != null && (currentNode == null || currentNode.autoAdvanceTime <= 0))
            {
                continueHint.SetActive(true);
            }

            // 如果文本显示完成且没有自动前进，可以继续
            if (currentNode != null && currentNode.autoAdvanceTime <= 0)
            {
                // 等待用户点击
            }
        }

        private IEnumerator AutoAdvance(float duration)
        {
            float timer = 0f;

            // 更新进度条
            if (autoAdvanceSlider != null)
            {
                autoAdvanceSlider.gameObject.SetActive(true);
                autoAdvanceSlider.value = 0f;
            }

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = timer / duration;

                if (autoAdvanceSlider != null)
                {
                    autoAdvanceSlider.value = progress;
                }

                yield return null;
            }

            // 自动前进完成
            if (autoAdvanceSlider != null)
            {
                autoAdvanceSlider.gameObject.SetActive(false);
            }

            autoAdvanceCoroutine = null;
            CompleteDialogue();
        }

        private void CompleteDialogue()
        {
            OnDialogueCompleted?.Invoke();
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null)
                yield break;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
                yield break;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}
