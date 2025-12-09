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
        
        [Header("跟随设置")]
        [Tooltip("跟随目标（如宠物）")]
        private Transform followTarget;
        
        [Tooltip("相对于目标的偏移（世界坐标）")]
        public Vector3 worldOffset = new Vector3(0, 100, 0);
        
        [Tooltip("是否每帧更新位置")]
        public bool updateEveryFrame = true;
        
        // 缓存的RectTransform
        private RectTransform cachedRectTransform;

        public override bool IsShowing => isShowing;
        public override DialogueDisplayMode DisplayMode => DialogueDisplayMode.Bubble;
        
        /// <summary>
        /// 设置跟随目标
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            
            if (followTarget != null)
            {
                StartCoroutine(UpdatePositionNextFrame());
            }
        }
        
        /// <summary>
        /// 在下一帧更新位置
        /// </summary>
        private System.Collections.IEnumerator UpdatePositionNextFrame()
        {
            yield return null;
            UpdatePosition();
        }
        
        /// <summary>
        /// 查找可用的RectTransform
        /// </summary>
        private RectTransform FindRectTransform()
        {
            RectTransform rect = transform as RectTransform;
            if (rect != null) return rect;
            
            rect = GetComponent<RectTransform>();
            if (rect != null) return rect;
            
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    rect = transform.GetChild(i).GetComponent<RectTransform>();
                    if (rect != null) return rect;
                }
            }
            
            rect = GetComponentInChildren<RectTransform>(true);
            if (rect != null) return rect;
            
            Debug.LogError($"[BubbleDialogueUI] 找不到任何RectTransform！");
            return null;
        }

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
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
            }
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

            // 如果bubblePanel为null，重新查找
            if (bubblePanel == null)
            {
                AutoFindUIComponents();
            }

            // 显示面板
            if (bubblePanel != null)
            {
                bubblePanel.SetActive(true);
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
            }

            // 开始打字动画
            float textSpeed = node.textSpeed > 0 ? node.textSpeed : defaultTextSpeed;
            if (textSpeed <= 0) textSpeed = 9999f;

            if (bubbleText != null)
            {
                textTypingCoroutine = StartCoroutine(TypeTextCoroutine(bubbleText, node.text, textSpeed));
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
            }

            // 淡入动画
            StartCoroutine(FadeInCoroutine());
        }

        public override void HideDialogue()
        {
            isShowing = false;
            followTarget = null;
            StopAllCoroutines();

            StartCoroutine(FadeOutCoroutine());
        }
        
        /// <summary>
        /// 更新UI位置以跟随目标
        /// </summary>
        private void UpdatePosition()
        {
            if (followTarget == null) return;
            
            // 如果还没有缓存RectTransform，先查找并缓存
            if (cachedRectTransform == null)
            {
                cachedRectTransform = FindRectTransform();
                
                if (cachedRectTransform == null)
                {
                    followTarget = null;
                    updateEveryFrame = false;
                    return;
                }
            }
            
            RectTransform rectTransform = cachedRectTransform;
            
            // 获取Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            // 检查目标是否也是RectTransform（UI对象，如桌宠）
            RectTransform targetRect = followTarget.GetComponent<RectTransform>();
            
            if (targetRect != null)
            {
                // 目标是UI对象（如桌宠在Canvas下）
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Vector3 targetWorldPos = targetRect.position;
                    Vector2 offset2D = new Vector2(worldOffset.x, worldOffset.y);
                    Vector3 newPosition = targetWorldPos + (Vector3)offset2D;
                    rectTransform.position = newPosition;
                }
                else
                {
                    Vector2 targetPosition = targetRect.anchoredPosition;
                    Vector2 newPosition = targetPosition + new Vector2(worldOffset.x, worldOffset.y);
                    rectTransform.anchoredPosition = newPosition;
                }
            }
            else
            {
                // 目标是世界空间对象（如3D角色）
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Camera cam = canvas.worldCamera ?? Camera.main;
                    Vector3 worldPosition = followTarget.position + worldOffset;
                    Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
                    rectTransform.position = screenPosition;
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    Camera cam = canvas.worldCamera ?? Camera.main;
                    if (cam != null)
                    {
                        Vector3 worldPosition = followTarget.position + worldOffset;
                        Vector3 screenPoint = cam.WorldToScreenPoint(worldPosition);
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            rectTransform.parent as RectTransform,
                            screenPoint,
                            cam,
                            out localPoint
                        );
                        rectTransform.localPosition = localPoint;
                    }
                }
            }
        }

        private IEnumerator AutoHideCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            autoHideCoroutine = null;
            
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
            // 每帧更新位置以跟随目标
            if (isShowing && updateEveryFrame && followTarget != null)
            {
                UpdatePosition();
            }
            
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
