using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace NewDialogueSystem
{
    /// <summary>
    /// 气泡对话UI（桌宠模式）
    /// </summary>
    public class BubbleDialogueUI : DialogueUIBase
    {
        [Header("UI组件引用")]
        [Tooltip("气泡面板")]
        public GameObject bubblePanel;

        [Tooltip("气泡文本")]
        public TextMeshProUGUI bubbleText;

        [Tooltip("自动隐藏时间（秒，0表示需要手动点击）")]
        public float autoHideTime = 4f;

        private Coroutine autoHideCoroutine;

        public override bool IsShowing => isShowing;
        public override DialogueDisplayMode DisplayMode => DialogueDisplayMode.Bubble;

        protected override void Awake()
        {
            Debug.Log("[BubbleDialogueUI] Awake() 开始执行");
            base.Awake();

            // 自动查找UI组件
            AutoFindUIComponents();
            
            Debug.Log($"[BubbleDialogueUI] UI组件查找完成，bubblePanel: {(bubblePanel != null ? bubblePanel.name : "null")}，bubbleText: {(bubbleText != null ? bubbleText.name : "null")}");

            // 初始化时隐藏
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
                Debug.Log("[BubbleDialogueUI] 初始化时隐藏bubblePanel");
            }
            else
            {
                Debug.LogWarning("[BubbleDialogueUI] Awake() 时未找到bubblePanel！");
            }
            
            Debug.Log("[BubbleDialogueUI] Awake() 完成");
        }

        /// <summary>
        /// 自动查找UI组件
        /// </summary>
        private void AutoFindUIComponents()
        {
            if (bubblePanel == null)
            {
                bubblePanel = FindPanelRecursive(transform);
            }

            if (bubblePanel != null && bubbleText == null)
            {
                bubbleText = bubblePanel.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 递归查找Panel
        /// </summary>
        private GameObject FindPanelRecursive(Transform parent)
        {
            if (parent == null) return null;

            // 尝试直接查找
            Transform panel = parent.Find("Panel");
            if (panel != null)
                return panel.gameObject;

            // 递归查找所有子对象
            foreach (Transform child in parent)
            {
                GameObject found = FindPanelRecursive(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        public override void ShowDialogue(DialogueNode node, CharacterData character)
        {
            Debug.Log($"[BubbleDialogueUI] ShowDialogue 被调用，节点: {node?.nodeID}");
            
            if (node == null)
            {
                Debug.LogError("[BubbleDialogueUI] 节点为null！");
                return;
            }

            currentNode = node;
            currentCharacter = character;

            // 停止之前的协程
            StopAllCoroutines();

            // 确保父对象激活
            EnsureParentActive();
            
            Debug.Log($"[BubbleDialogueUI] 父对象激活检查完成，GameObject激活: {gameObject.activeSelf}，场景: {gameObject.scene.name}");

            // 如果bubblePanel为null，重新查找
            if (bubblePanel == null)
            {
                Debug.LogWarning("[BubbleDialogueUI] bubblePanel为null，重新查找...");
                AutoFindUIComponents();
            }

            // 显示面板
            if (bubblePanel != null)
            {
                Debug.Log($"[BubbleDialogueUI] 激活bubblePanel: {bubblePanel.name}，当前状态: {bubblePanel.activeSelf}");
                bubblePanel.SetActive(true);
                Debug.Log($"[BubbleDialogueUI] bubblePanel激活后状态: {bubblePanel.activeSelf}");
            }
            else
            {
                Debug.LogError("[BubbleDialogueUI] bubblePanel为null，无法显示！");
                return;
            }

            // 设置文本颜色
            if (bubbleText != null && character != null)
            {
                bubbleText.color = character.textColor;
                Debug.Log($"[BubbleDialogueUI] 设置文本颜色: {character.textColor}");
            }
            else
            {
                if (bubbleText == null)
                    Debug.LogError("[BubbleDialogueUI] bubbleText为null！");
                if (character == null)
                    Debug.LogWarning("[BubbleDialogueUI] character为null，使用默认颜色");
            }

            // 开始打字动画
            float textSpeed = node.textSpeed > 0 ? node.textSpeed : defaultTextSpeed;
            if (textSpeed <= 0)
                textSpeed = 9999f; // 立即显示

            Debug.Log($"[BubbleDialogueUI] 开始打字动画，文本: {node.text}，速度: {textSpeed}");
            if (bubbleText != null)
            {
                textTypingCoroutine = StartCoroutine(TypeTextCoroutine(bubbleText, node.text, textSpeed));
            }
            else
            {
                Debug.LogError("[BubbleDialogueUI] bubbleText为null，无法开始打字动画！");
            }

            // 设置自动隐藏
            float hideTime = node.autoAdvanceTime > 0 ? node.autoAdvanceTime : autoHideTime;
            if (hideTime > 0)
            {
                autoHideCoroutine = StartCoroutine(AutoHideCoroutine(hideTime));
            }

            isShowing = true;

            // 确保UI可见
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log($"[BubbleDialogueUI] 设置CanvasGroup alpha: {canvasGroup.alpha}");
            }

            // 检查Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"[BubbleDialogueUI] 找到Canvas: {canvas.name}，RenderMode: {canvas.renderMode}，SortingOrder: {canvas.sortingOrder}，启用: {canvas.enabled}");
                if (!canvas.enabled)
                {
                    Debug.LogWarning("[BubbleDialogueUI] Canvas未启用，尝试启用...");
                    canvas.enabled = true;
                }
            }
            else
            {
                Debug.LogError("[BubbleDialogueUI] 未找到Canvas！UI可能无法显示！");
            }

            // 淡入动画
            StartCoroutine(FadeInCoroutine());
            
            Debug.Log("[BubbleDialogueUI] ShowDialogue 完成");
        }

        public override void HideDialogue()
        {
            isShowing = false;
            StopAllCoroutines();

            // 淡出动画
            StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator AutoHideCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            autoHideCoroutine = null;
            
            // 等待打字完成
            while (textTypingCoroutine != null)
            {
                yield return null;
            }

            CompleteDialogue();
        }

        protected override void OnTypingComplete()
        {
            base.OnTypingComplete();
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
                    // 已完成打字，继续下一个或隐藏
                    CompleteDialogue();
                }
            }
        }

        private void CompleteDialogue()
        {
            OnDialogueCompleted?.Invoke();
        }

        protected override IEnumerator FadeOutCoroutine()
        {
            yield return base.FadeOutCoroutine();

            // 淡出后隐藏面板
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
            }
        }
    }
}

