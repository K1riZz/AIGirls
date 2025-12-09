using UnityEngine;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 角色数据（扩展版）
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        [Header("基础信息")]
        [Tooltip("角色ID（唯一标识）")]
        public string characterID;

        [Tooltip("角色名称")]
        public string characterName;

        [Tooltip("角色描述")]
        [TextArea(2, 5)]
        public string description;

        [Header("视觉资源")]
        [Tooltip("角色默认头像/立绘路径（相对于Resources文件夹）")]
        public string defaultPortraitPath;

        [Tooltip("角色头像/立绘Sprite列表（可在代码中直接设置）")]
        public List<Sprite> portraitSprites = new List<Sprite>();

        [Tooltip("角色表情映射（表情名称 -> Sprite路径）")]
        public Dictionary<string, string> emotionPortraits = new Dictionary<string, string>();

        [Header("UI显示")]
        [Tooltip("角色名称颜色（用于UI显示）")]
        public Color nameColor = Color.white;

        [Tooltip("角色对话文本颜色（用于UI显示）")]
        public Color textColor = Color.white;

        [Tooltip("角色名称字体大小（0表示使用UI默认）")]
        public int nameFontSize = 0;

        [Tooltip("角色对话文本字体大小（0表示使用UI默认）")]
        public int textFontSize = 0;

        [Header("音频")]
        [Tooltip("角色语音音量（0-1）")]
        [Range(0f, 1f)]
        public float voiceVolume = 1f;

        [Tooltip("角色默认语音音调")]
        [Range(0.5f, 2f)]
        public float voicePitch = 1f;

        [Tooltip("角色语音列表（语音ID -> 语音路径）")]
        public Dictionary<string, string> voiceClips = new Dictionary<string, string>();

        [Header("行为")]
        [Tooltip("角色是否可以被跳过")]
        public bool canSkip = true;

        [Tooltip("角色默认对话速度（字符/秒）")]
        public float defaultTextSpeed = 30f;

        /// <summary>
        /// 获取角色立绘Sprite（从Resources加载）
        /// </summary>
        public Sprite GetPortraitSprite(string emotion = null)
        {
            // 如果指定了表情，尝试加载表情立绘
            if (!string.IsNullOrEmpty(emotion) && emotionPortraits.ContainsKey(emotion))
            {
                string path = emotionPortraits[emotion];
                if (!string.IsNullOrEmpty(path))
                {
                    Sprite sprite = Resources.Load<Sprite>(path);
                    if (sprite != null)
                        return sprite;
                }
            }

            // 使用默认立绘
            if (portraitSprites.Count > 0)
                return portraitSprites[0];

            if (!string.IsNullOrEmpty(defaultPortraitPath))
            {
                Sprite sprite = Resources.Load<Sprite>(defaultPortraitPath);
                if (sprite != null)
                    return sprite;
            }

            return null;
        }

        /// <summary>
        /// 获取语音音频路径
        /// </summary>
        public string GetVoicePath(string voiceID = null)
        {
            if (!string.IsNullOrEmpty(voiceID) && voiceClips.ContainsKey(voiceID))
            {
                return voiceClips[voiceID];
            }
            return null;
        }
    }
}

