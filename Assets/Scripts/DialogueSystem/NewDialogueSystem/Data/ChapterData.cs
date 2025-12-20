using UnityEngine;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 章节数据
    /// </summary>
    [System.Serializable]
    public class ChapterData
    {
        [Tooltip("章节ID（唯一标识）")]
        public string chapterID;

        [Tooltip("章节名称")]
        public string chapterName;

        [Tooltip("章节描述（可选）")]
        public string description;

        [Tooltip("章节图标路径（相对于Resources文件夹，可选）")]
        public string iconPath;

        [Tooltip("章节顺序（用于排序）")]
        public int order = 0;

        public ChapterData(string chapterID, string chapterName, int order = 0)
        {
            this.chapterID = chapterID;
            this.chapterName = chapterName;
            this.order = order;
        }
    }

    /// <summary>
    /// 章节数据库（从JSON加载或ScriptableObject配置）
    /// </summary>
    [System.Serializable]
    public class ChapterDatabase
    {
        public List<ChapterData> chapters = new List<ChapterData>();

        private Dictionary<string, ChapterData> chapterDict = new Dictionary<string, ChapterData>();

        /// <summary>
        /// 初始化（建立字典索引）
        /// </summary>
        public void Initialize()
        {
            chapterDict.Clear();
            foreach (var chapter in chapters)
            {
                if (!string.IsNullOrEmpty(chapter.chapterID))
                {
                    chapterDict[chapter.chapterID] = chapter;
                }
            }
        }

        /// <summary>
        /// 获取章节数据
        /// </summary>
        public ChapterData GetChapter(string chapterID)
        {
            chapterDict.TryGetValue(chapterID, out ChapterData chapter);
            return chapter;
        }

        /// <summary>
        /// 获取所有章节（按顺序排序）
        /// </summary>
        public List<ChapterData> GetAllChapters()
        {
            var sorted = new List<ChapterData>(chapters);
            sorted.Sort((a, b) => a.order.CompareTo(b.order));
            return sorted;
        }
    }
}


