using UnityEngine;
using System.Collections.Generic;

namespace GalDialogueSystem
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
        End         // 对话结束
    }

    /// <summary>
    /// 对话模式
    /// </summary>
    public enum DialogueMode
    {
        Story,      // 剧情对话（全屏AVG模式）
        Bubble,     // 气泡对话（桌宠模式）
        Notification, // 通知模式
        Custom      // 自定义模式
    }

    /// <summary>
    /// 对话节点数据
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        [Tooltip("节点ID（唯一标识）")]
        public string nodeID;

        [Tooltip("节点类型")]
        public DialogueNodeType nodeType = DialogueNodeType.Text;

        [Tooltip("对话模式")]
        public DialogueMode dialogueMode = DialogueMode.Story;

        [Tooltip("角色ID（如果为空则使用默认角色）")]
        public string characterID;

        [Tooltip("对话文本内容")]
        [TextArea(3, 10)]
        public string text;

        [Tooltip("角色名称（如果为空则使用角色的默认名称）")]
        public string characterName;

        [Tooltip("角色头像/立绘（可选）")]
        public string portraitSpritePath;

        [Tooltip("背景图片路径（可选）")]
        public string backgroundImagePath;

        [Tooltip("插入的图片路径（可选）")]
        public string insertImagePath;

        [Tooltip("选择项列表（仅用于Choice类型节点）")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Tooltip("下一个节点ID（用于Text和Event类型）")]
        public string nextNodeID;

        [Tooltip("触发的事件名称（可选）")]
        public string eventName;

        [Tooltip("事件参数（JSON格式，可选）")]
        public string eventData;

        [Tooltip("显示条件（Lua表达式或JSON条件，可选）")]
        public string condition;

        [Tooltip("对话速度（字符/秒，0表示立即显示）")]
        public float textSpeed = 30f;

        [Tooltip("自动前进时间（秒，0表示需要手动点击）")]
        public float autoAdvanceTime = 0f;

        [Tooltip("音效路径（可选）")]
        public string soundEffectPath;

        [Tooltip("背景音乐路径（可选）")]
        public string backgroundMusicPath;

        /// <summary>
        /// 检查节点是否可以显示（检查条件）
        /// </summary>
        public bool CanDisplay()
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // TODO: 实现条件检查逻辑（Lua表达式或JSON条件）
            // 暂时返回true
            return true;
        }
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

        [Tooltip("显示条件（可选）")]
        public string condition;

        [Tooltip("选择后的特殊效果（可选）")]
        public string effect;

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
