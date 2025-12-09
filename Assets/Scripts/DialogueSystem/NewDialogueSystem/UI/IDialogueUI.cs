namespace NewDialogueSystem
{
    /// <summary>
    /// 对话UI接口（所有对话UI必须实现此接口）
    /// </summary>
    public interface IDialogueUI
    {
        /// <summary>
        /// 是否正在显示
        /// </summary>
        bool IsShowing { get; }

        /// <summary>
        /// 对话显示模式
        /// </summary>
        DialogueDisplayMode DisplayMode { get; }

        /// <summary>
        /// 对话完成回调
        /// </summary>
        System.Action OnDialogueCompleted { get; set; }

        /// <summary>
        /// 显示对话
        /// </summary>
        void ShowDialogue(DialogueNode node, CharacterData character);

        /// <summary>
        /// 隐藏对话
        /// </summary>
        void HideDialogue();
    }

    /// <summary>
    /// 选择对话UI接口
    /// </summary>
    public interface IChoiceDialogueUI : IDialogueUI
    {
        /// <summary>
        /// 显示选择项
        /// </summary>
        void ShowChoices(System.Collections.Generic.List<DialogueChoice> choices, System.Action<DialogueChoice> onChoiceSelected);

        /// <summary>
        /// 隐藏选择项
        /// </summary>
        void HideChoices();
    }

    /// <summary>
    /// 历史记录UI接口
    /// </summary>
    public interface IHistoryDialogueUI
    {
        /// <summary>
        /// 显示历史记录
        /// </summary>
        void ShowHistory();

        /// <summary>
        /// 隐藏历史记录
        /// </summary>
        void HideHistory();

        /// <summary>
        /// 添加历史记录条目
        /// </summary>
        void AddHistoryEntry(DialogueHistoryEntry entry);
    }
}

