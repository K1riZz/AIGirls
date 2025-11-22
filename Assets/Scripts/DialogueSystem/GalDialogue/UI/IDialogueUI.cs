using UnityEngine;

namespace GalDialogueSystem
{
    /// <summary>
    /// 对话UI接口（所有对话UI都必须实现此接口）
    /// </summary>
    public interface IDialogueUI
    {
        /// <summary>
        /// 显示对话节点
        /// </summary>
        void ShowDialogue(DialogueNode node, CharacterData character);

        /// <summary>
        /// 隐藏对话UI
        /// </summary>
        void HideDialogue();

        /// <summary>
        /// 对话是否正在显示
        /// </summary>
        bool IsShowing { get; }

        /// <summary>
        /// 对话模式
        /// </summary>
        DialogueMode DialogueMode { get; }

        /// <summary>
        /// 完成当前对话（由UI调用，通知管理器继续）
        /// </summary>
        System.Action OnDialogueCompleted { get; set; }
    }
}
