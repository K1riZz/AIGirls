using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GalDialogueSystem
{
    /// <summary>
    /// 气泡对话UI（桌宠模式，类似之前的气泡对话）
    /// </summary>
    public class BubbleDialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("UI组件引用")]
        [Tooltip("气泡面板")]
        public GameObject bubblePanel;

        [Tooltip("气泡文本")]
        public TextMeshProUGUI bubbleText;

        [Tooltip("气泡背景图片")]
        public Image bubbleBackground;

        [Header("动画设置")]
        [Tooltip("淡入淡出时间")]
        public float fadeDuration = 0.3f;

        [Tooltip("文字显示动画速度（字符/秒）")]
        public float defaultTextSpeed = 50f;

        [Tooltip("自动隐藏时间（秒，0表示需要手动点击）")]
        public float autoHideTime = 4f;

        private DialogueMode dialogueMode = DialogueMode.Bubble;
        private bool isShowing = false;
        private DialogueNode currentNode;
        private CharacterData currentCharacter;
        private Coroutine textTypingCoroutine;
        private Coroutine autoHideCoroutine;
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
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
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
                    if (bubbleText != null && currentNode != null)
                    {
                        bubbleText.text = currentNode.text;
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
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

            // 显示UI
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(true);
            }

            // 设置文本颜色
            if (bubbleText != null && character != null)
            {
                bubbleText.color = character.textColor;
            }

            // 开始打字动画
            float textSpeed = node.textSpeed > 0 ? node.textSpeed : defaultTextSpeed;
            textTypingCoroutine = StartCoroutine(TypeText(node.text, textSpeed));

            // 设置自动隐藏
            float hideTime = node.autoAdvanceTime > 0 ? node.autoAdvanceTime : autoHideTime;
            if (hideTime > 0)
            {
                autoHideCoroutine = StartCoroutine(AutoHide(hideTime));
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
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

            // 淡出动画
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
            }
        }

        private IEnumerator TypeText(string text, float speed)
        {
            if (bubbleText == null)
                yield break;

            bubbleText.text = "";
            float charDelay = 1f / speed;

            foreach (char c in text)
            {
                bubbleText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            OnTypingComplete();
        }

        private void OnTypingComplete()
        {
            textTypingCoroutine = null;
        }

        private IEnumerator AutoHide(float duration)
        {
            yield return new WaitForSeconds(duration);
            autoHideCoroutine = null;
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

            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
            }
        }
    }
}
