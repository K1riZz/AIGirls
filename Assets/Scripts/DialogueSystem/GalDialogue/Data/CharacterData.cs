using UnityEngine;

namespace GalDialogueSystem
{
    /// <summary>
    /// 角色数据
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        [Tooltip("角色ID（唯一标识）")]
        public string characterID;

        [Tooltip("角色名称")]
        public string characterName;

        [Tooltip("角色默认头像/立绘路径")]
        public string defaultPortraitPath;

        [Tooltip("角色名称颜色（用于UI显示）")]
        public Color nameColor = Color.white;

        [Tooltip("角色对话文本颜色（用于UI显示）")]
        public Color textColor = Color.white;

        [Tooltip("角色语音音量（0-1）")]
        [Range(0f, 1f)]
        public float voiceVolume = 1f;

        [Tooltip("角色默认语音音调（可选）")]
        public float voicePitch = 1f;

        /// <summary>
        /// 获取角色立绘Sprite（从Resources加载）
        /// </summary>
        public Sprite GetPortraitSprite()
        {
            if (string.IsNullOrEmpty(defaultPortraitPath))
                return null;

            return Resources.Load<Sprite>(defaultPortraitPath);
        }
    }
}
