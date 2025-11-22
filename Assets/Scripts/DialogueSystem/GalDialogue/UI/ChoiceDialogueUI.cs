using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace GalDialogueSystem
{
    /// <summary>
    /// 选择对话UI（多重选择）
    /// </summary>
    public class ChoiceDialogueUI : MonoBehaviour
    {
        [Header("UI组件引用")]
        [Tooltip("选择面板")]
        public GameObject choicePanel;

        [Tooltip("选择按钮预制体")]
        public GameObject choiceButtonPrefab;

        [Tooltip("选择按钮容器")]
        public Transform choiceButtonContainer;

        [Header("动画设置")]
        [Tooltip("淡入淡出时间")]
        public float fadeDuration = 0.3f;

        private List<Button> choiceButtons = new List<Button>();
        private System.Action<DialogueChoice> OnChoiceSelected;

        void Awake()
        {
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示选择项
        /// </summary>
        public void ShowChoices(List<DialogueChoice> choices, System.Action<DialogueChoice> onChoiceSelected)
        {
            if (choices == null || choices.Count == 0)
            {
                Debug.LogWarning("[ChoiceDialogueUI] 选择项列表为空");
                return;
            }

            OnChoiceSelected = onChoiceSelected;

            // 清除之前的按钮
            ClearChoices();

            // 创建选择按钮
            foreach (var choice in choices)
            {
                if (!choice.CanDisplay())
                    continue;

                GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = choice.text;
                }

                if (button != null)
                {
                    DialogueChoice choiceCopy = choice; // 闭包捕获
                    button.onClick.AddListener(() => OnChoiceButtonClicked(choiceCopy));
                    choiceButtons.Add(button);
                }
            }

            // 显示面板
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏选择UI
        /// </summary>
        public void HideChoices()
        {
            ClearChoices();

            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 清除所有选择按钮
        /// </summary>
        private void ClearChoices()
        {
            foreach (var button in choiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            choiceButtons.Clear();

            // 清理容器中的子对象
            if (choiceButtonContainer != null)
            {
                foreach (Transform child in choiceButtonContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 选择按钮点击事件
        /// </summary>
        private void OnChoiceButtonClicked(DialogueChoice choice)
        {
            OnChoiceSelected?.Invoke(choice);
            HideChoices();
        }
    }
}
