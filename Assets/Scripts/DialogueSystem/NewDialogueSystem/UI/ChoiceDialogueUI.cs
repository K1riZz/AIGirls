using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 选择对话UI（用于显示选择分支）
    /// </summary>
    public class ChoiceDialogueUI : DialogueUIBase, IChoiceDialogueUI
    {
        [Header("UI组件引用")]
        [Tooltip("选择面板")]
        public GameObject choicePanel;

        [Tooltip("选择按钮容器（用于放置选择按钮）")]
        public Transform choiceButtonContainer;

        [Tooltip("选择按钮预制体（如果为空则使用默认按钮样式）")]
        public GameObject choiceButtonPrefab;

        [Header("选择项设置")]
        [Tooltip("按钮间距")]
        public float buttonSpacing = 10f;

        private List<GameObject> currentChoiceButtons = new List<GameObject>();
        private System.Action<DialogueChoice> onChoiceSelectedCallback;

        public override bool IsShowing => isShowing;
        public override DialogueDisplayMode DisplayMode => DialogueDisplayMode.Custom;

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
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 自动查找UI组件
        /// </summary>
        private void AutoFindUIComponents()
        {
            if (choicePanel == null)
            {
                choicePanel = FindPanelRecursive(transform);
            }

            if (choiceButtonContainer == null)
            {
                Transform container = transform.Find("ChoiceButtonContainer");
                if (container == null)
                {
                    container = FindChildRecursive(transform, "ChoiceButtonContainer");
                }
                if (container != null)
                {
                    choiceButtonContainer = container;
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
                if (child.name == name) return child;

                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }

        public override void ShowDialogue(DialogueNode node, CharacterData character)
        {
            // 选择UI主要用于显示选择项，ShowDialogue可以留空
            // 实际的显示逻辑在ShowChoices中
        }

        public override void HideDialogue()
        {
            isShowing = false;
            StopAllCoroutines();

            HideChoices();
            StartCoroutine(FadeOutCoroutine());
        }

        /// <summary>
        /// 显示选择项（实现IChoiceDialogueUI接口）
        /// </summary>
        public void ShowChoices(List<DialogueChoice> choices, System.Action<DialogueChoice> onChoiceSelected)
        {
            if (choices == null || choices.Count == 0)
            {
                Debug.LogWarning("[ChoiceDialogueUI] 选择项列表为空");
                return;
            }

            // 保存回调
            this.onChoiceSelectedCallback = onChoiceSelected;

            // 确保父对象激活
            EnsureParentActive();

            // 显示面板
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }

            // 清除之前的选择按钮
            ClearChoiceButtons();

            // 如果没有按钮容器，创建一个
            if (choiceButtonContainer == null)
            {
                CreateButtonContainer();
            }

            // 创建选择按钮
            for (int i = 0; i < choices.Count; i++)
            {
                DialogueChoice choice = choices[i];

                if (!choice.CanDisplay()) continue;

                GameObject buttonObj = CreateChoiceButton(choice, i);
                if (buttonObj != null)
                {
                    currentChoiceButtons.Add(buttonObj);
                }
            }

            isShowing = true;

            // 确保UI可以交互
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            // 确保Canvas有最高的sorting order
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (canvas.sortingOrder < 2000)
                {
                    canvas.sortingOrder = 2000;
                    canvas.overrideSorting = true;
                }
                
                if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
            }

            // 淡入动画
            StartCoroutine(FadeInCoroutine());
        }

        /// <summary>
        /// 隐藏选择项（实现IChoiceDialogueUI接口）
        /// </summary>
        public void HideChoices()
        {
            ClearChoiceButtons();

            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }

            onChoiceSelectedCallback = null;
        }

        /// <summary>
        /// 创建按钮容器
        /// </summary>
        private void CreateButtonContainer()
        {
            if (choicePanel == null)
            {
                Debug.LogError("[ChoiceDialogueUI] 无法创建按钮容器：choicePanel为null");
                return;
            }

            GameObject containerObj = new GameObject("ChoiceButtonContainer");
            containerObj.transform.SetParent(choicePanel.transform, false);

            RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(400, 300);
            rectTransform.anchoredPosition = Vector2.zero;

            // 添加Vertical Layout Group
            VerticalLayoutGroup layoutGroup = containerObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = buttonSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            choiceButtonContainer = containerObj.transform;
        }

        /// <summary>
        /// 创建选择按钮
        /// </summary>
        private GameObject CreateChoiceButton(DialogueChoice choice, int index)
        {
            GameObject buttonObj = null;

            // 使用预制体或创建默认按钮
            if (choiceButtonPrefab != null)
            {
                buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            }
            else
            {
                buttonObj = CreateDefaultButton();
            }

            if (buttonObj == null)
            {
                Debug.LogError("[ChoiceDialogueUI] 无法创建选择按钮");
                return null;
            }

            buttonObj.name = $"ChoiceButton_{index}";
            buttonObj.SetActive(true);

            // 设置按钮文本
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                buttonText = buttonObj.GetComponent<TextMeshProUGUI>();
            }

            if (buttonText != null)
            {
                buttonText.text = choice.text;
                buttonText.raycastTarget = false;
            }
            else
            {
                // 创建文本对象
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = choice.text;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontSize = 24;
                buttonText.raycastTarget = false;
            }

            // 设置按钮点击事件
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }

            button.interactable = true;
            
            // 确保按钮有Graphic组件（Button需要Image或Text作为targetGraphic）
            if (button.targetGraphic == null)
            {
                Image image = buttonObj.GetComponent<Image>();
                if (image == null)
                {
                    image = buttonObj.AddComponent<Image>();
                    image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                }
                button.targetGraphic = image;
            }

            // 确保按钮的 RectTransform 设置正确
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                if (buttonRect.sizeDelta.x <= 0 || buttonRect.sizeDelta.y <= 0)
                {
                    buttonRect.sizeDelta = new Vector2(350, 60);
                }
                
                buttonRect.localScale = Vector3.one;
            }

            // 清除旧的监听器并添加新的
            button.onClick.RemoveAllListeners();
            
            DialogueChoice capturedChoice = choice;
            button.onClick.AddListener(() => OnChoiceButtonClicked(capturedChoice));

            // 确保按钮对象及其所有父对象都是激活的
            Transform checkParent = buttonObj.transform;
            while (checkParent != null)
            {
                if (!checkParent.gameObject.activeSelf)
                {
                    checkParent.gameObject.SetActive(true);
                }
                
                // 检查CanvasGroup
                CanvasGroup parentGroup = checkParent.GetComponent<CanvasGroup>();
                if (parentGroup != null)
                {
                    parentGroup.interactable = true;
                    parentGroup.blocksRaycasts = true;
                }
                
                checkParent = checkParent.parent;
            }

            // 确保按钮本身也有正确的CanvasGroup设置（如果有）
            CanvasGroup buttonGroup = buttonObj.GetComponent<CanvasGroup>();
            if (buttonGroup != null)
            {
                buttonGroup.interactable = true;
                buttonGroup.blocksRaycasts = true;
            }

            return buttonObj;
        }

        /// <summary>
        /// 创建默认按钮样式
        /// </summary>
        private GameObject CreateDefaultButton()
        {
            GameObject buttonObj = new GameObject("ChoiceButton");
            buttonObj.transform.SetParent(choiceButtonContainer, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(350, 60);
            rectTransform.anchoredPosition = Vector2.zero;

            // 添加Image组件作为背景
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();

            // 创建文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "选择项";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.white;
            text.raycastTarget = false;

            return buttonObj;
        }

        /// <summary>
        /// 选择按钮点击事件
        /// </summary>
        private void OnChoiceButtonClicked(DialogueChoice choice)
        {
            if (onChoiceSelectedCallback != null)
            {
                onChoiceSelectedCallback(choice);
            }
            else
            {
                Debug.LogError("[ChoiceDialogueUI] onChoiceSelectedCallback 为 null！");
            }

            HideChoices();
        }

        /// <summary>
        /// 清除所有选择按钮
        /// </summary>
        private void ClearChoiceButtons()
        {
            foreach (GameObject button in currentChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            currentChoiceButtons.Clear();
        }
    }
}
