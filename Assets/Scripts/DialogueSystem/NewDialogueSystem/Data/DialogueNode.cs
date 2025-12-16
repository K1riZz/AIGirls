using UnityEngine;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话节点类型
    /// </summary>
    public enum DialogueNodeType
    {
        Text,       // 普通文本对话
        Choice,     // 选择分支
        Image,      // 图片插入
        Event,      // 事件触发
        Jump,       // 跳转到其他节点
        End         // 对话结束
    }

    /// <summary>
    /// 对话显示模式
    /// </summary>
    public enum DialogueDisplayMode
    {
        Story,          // 剧情对话（全屏AVG模式）
        Bubble,         // 气泡对话（桌宠模式）
        Notification,   // 通知模式
        SidePanel,      // 侧边面板
        Custom          // 自定义模式
    }

    /// <summary>
    /// 对话节点数据（核心数据结构）
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        [Header("基础信息")]
        [Tooltip("节点ID（唯一标识，必填）")]
        public string nodeID;

        [Tooltip("节点类型")]
        public DialogueNodeType nodeType = DialogueNodeType.Text;

        [Tooltip("对话显示模式")]
        public DialogueDisplayMode displayMode = DialogueDisplayMode.Story;

        [Tooltip("角色ID（如果为空则使用默认角色）")]
        public string characterID;

        [Tooltip("角色名称覆盖（如果为空则使用角色的默认名称）")]
        public string characterNameOverride;

        [Tooltip("章节ID（用于历史对话分组）")]
        public string chapterID;

        [Header("对话内容")]
        [Tooltip("对话文本内容")]
        [TextArea(3, 10)]
        public string text;

        [Tooltip("文本显示速度（字符/秒，0表示立即显示）")]
        public float textSpeed = 30f;

        [Tooltip("自动前进时间（秒，0表示需要手动点击）")]
        public float autoAdvanceTime = 0f;

        [Header("图片资源")]
        [Tooltip("角色头像/立绘路径（相对于Resources文件夹）")]
        public string portraitSpritePath;

        [Tooltip("背景图片路径（相对于Resources文件夹）")]
        public string backgroundImagePath;

        [Tooltip("插入的图片路径（相对于Resources文件夹，可多张）")]
        public List<string> insertImagePaths = new List<string>();

        [Header("音频资源")]
        [Tooltip("语音路径（相对于Resources文件夹）")]
        public string voicePath;

        [Tooltip("音效路径（相对于Resources文件夹）")]
        public string soundEffectPath;

        [Tooltip("背景音乐路径（相对于Resources文件夹）")]
        public string backgroundMusicPath;

        [Header("分支和流程")]
        [Tooltip("选择项列表（仅用于Choice类型节点）")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Tooltip("下一个节点ID（用于Text和Event类型）")]
        public string nextNodeID;

        [Tooltip("条件分支：条件表达式和对应的下一个节点ID")]
        public List<ConditionalBranch> conditionalBranches = new List<ConditionalBranch>();

        [Header("事件和条件")]
        [Tooltip("触发的事件名称（可选）")]
        public string eventName;

        [Tooltip("事件参数（JSON格式，可选）")]
        public string eventData;

        [Tooltip("显示条件（JSON格式，可选）")]
        public string condition;

        [Tooltip("执行前运行的脚本/函数名（可选）")]
        public string onEnterScript;

        [Tooltip("执行后运行的脚本/函数名（可选）")]
        public string onExitScript;

        [Header("UI配置")]
        [Tooltip("使用的UI实例ID（为空则使用默认UI）")]
        public string uiInstanceID;

        [Tooltip("UI位置偏移（用于多UI显示）")]
        public Vector2 uiOffset = Vector2.zero;

        [Tooltip("UI优先级（数字越大越在上层）")]
        public int uiPriority = 0;

        /// <summary>
        /// 检查节点是否可以显示（检查条件）
        /// </summary>
        public bool CanDisplay()
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // TODO: 实现条件检查逻辑（JSON条件表达式）
            // 暂时返回true
            return true;
        }

        /// <summary>
        /// 获取角色名称（优先使用覆盖名称）
        /// </summary>
        public string GetCharacterName(CharacterData character)
        {
            if (!string.IsNullOrEmpty(characterNameOverride))
                return characterNameOverride;
            
            if (character != null)
                return character.characterName;
            
            return "未知";
        }
    }

    /// <summary>
    /// 条件分支
    /// </summary>
    [System.Serializable]
    public class ConditionalBranch
    {
        [Tooltip("条件表达式（JSON格式）")]
        public string condition;

        [Tooltip("满足条件时跳转到的节点ID")]
        public string nextNodeID;

        [Tooltip("条件优先级（数字越大越优先检查）")]
        public int priority = 0;
    }

    /// <summary>
    /// 对话选择项
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        [Tooltip("选择项文本")]
        public string text;

        [Tooltip("选择后跳转到的节点ID")]
        public string nextNodeID;

        [Tooltip("显示条件（JSON格式，可选）")]
        public string condition;

        [Tooltip("选择后的特殊效果/事件（可选）")]
        public string effect;

        [Tooltip("选择项图标路径（可选）")]
        public string iconPath;

        /// <summary>
        /// 检查选择项是否可以显示
        /// </summary>
        public bool CanDisplay()
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // TODO: 实现条件检查逻辑
            return true;
        }
    }
}

