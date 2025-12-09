using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话UI基类（提供通用功能）
    /// </summary>
    public abstract class DialogueUIBase : MonoBehaviour, IDialogueUI
    {
        [Header("基础设置")]
        [Tooltip("淡入淡出时间")]
        public float fadeDuration = 0.3f;

        [Tooltip("默认文本显示速度（字符/秒）")]
        public float defaultTextSpeed = 30f;

        protected bool isShowing = false;
        protected DialogueNode currentNode;
        protected CharacterData currentCharacter;
        protected Coroutine textTypingCoroutine;
        protected CanvasGroup canvasGroup;

        // IDialogueUI接口实现
        public abstract bool IsShowing { get; }
        public abstract DialogueDisplayMode DisplayMode { get; }
        public System.Action OnDialogueCompleted { get; set; }

        protected virtual void Awake()
        {
            // 确保GameObject激活
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            // 获取或添加CanvasGroup
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 确保初始alpha为1
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 确保所有父对象都是激活的
            EnsureParentActive();
        }

        /// <summary>
        /// 确保所有父对象都是激活的
        /// </summary>
        protected void EnsureParentActive()
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

            // 确保根对象激活
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 显示对话（抽象方法，由子类实现）
        /// </summary>
        public abstract void ShowDialogue(DialogueNode node, CharacterData character);

        /// <summary>
        /// 隐藏对话（抽象方法，由子类实现）
        /// </summary>
        public abstract void HideDialogue();

        /// <summary>
        /// 打字动画协程
        /// </summary>
        protected IEnumerator TypeTextCoroutine(TextMeshProUGUI textComponent, string text, float speed)
        {
            if (textComponent == null)
                yield break;

            textComponent.text = "";
            float charDelay = speed > 0 ? (1f / speed) : 0f;

            foreach (char c in text)
            {
                textComponent.text += c;
                if (charDelay > 0)
                {
                    yield return new WaitForSeconds(charDelay);
                }
            }

            OnTypingComplete();
        }

        /// <summary>
        /// 打字完成回调
        /// </summary>
        protected virtual void OnTypingComplete()
        {
            textTypingCoroutine = null;
        }

        /// <summary>
        /// 淡入动画
        /// </summary>
        protected virtual IEnumerator FadeInCoroutine()
        {
            if (canvasGroup == null)
                yield break;

            // 直接设置为可见（不执行淡入，确保UI立即显示）
            canvasGroup.alpha = 1f;
            yield break;

            // 如果需要淡入效果，取消下面的注释
            /*
            canvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            */
        }

        /// <summary>
        /// 淡出动画
        /// </summary>
        protected virtual IEnumerator FadeOutCoroutine()
        {
            if (canvasGroup == null)
                yield break;

            float timer = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
    }
}

