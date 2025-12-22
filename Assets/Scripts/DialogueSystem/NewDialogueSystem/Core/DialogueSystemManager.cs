using UnityEngine;
using System.Collections.Generic;

namespace NewDialogueSystem
{
    /// <summary>
    /// 对话系统管理器（主单例）
    /// 负责管理所有对话会话、UI实例、数据库等
    /// </summary>
    public class DialogueSystemManager : MonoBehaviour
    {
        public static DialogueSystemManager Instance { get; private set; }

        [Header("数据库配置")]
        [Tooltip("对话数据库（可动态加载）")]
        public DialogueDatabase dialogueDatabase;

        [Tooltip("数据库JSON文件路径（相对于Resources文件夹）")]
        public string databaseJsonPath;

        [Tooltip("章节数据库（用于历史对话分组）")]
        public ChapterDatabase chapterDatabase;

        [Header("UI配置")]
        [Tooltip("对话UI容器（所有对话UI的父对象）")]
        public Transform dialogueUIContainer;

        [Header("默认UI预制体")]
        [Tooltip("默认剧情对话UI预制体")]
        public GameObject defaultStoryDialogueUIPrefab;

        [Tooltip("默认气泡对话UI预制体")]
        public GameObject defaultBubbleDialogueUIPrefab;

        [Tooltip("默认选择对话UI预制体")]
        public GameObject defaultChoiceDialogueUIPrefab;

        [Tooltip("默认历史记录UI预制体")]
        public GameObject defaultHistoryDialogueUIPrefab;

        [Header("系统设置")]
        [Tooltip("是否自动保存对话历史")]
        public bool autoSaveHistory = true;

        [Tooltip("历史记录最大条数")]
        public int maxHistoryEntries = 1000;

        // 内部数据
        private Dictionary<string, DialogueSession> activeSessions = new Dictionary<string, DialogueSession>();
        private Dictionary<string, IDialogueUI> uiInstances = new Dictionary<string, IDialogueUI>();
        private List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();
        private Dictionary<DialogueDisplayMode, GameObject> defaultUIPrefabs = new Dictionary<DialogueDisplayMode, GameObject>();
        
        // 章节解锁状态（已访问过的章节ID集合）
        private HashSet<string> unlockedChapters = new HashSet<string>();

        // 事件
        public System.Action<DialogueNode> OnDialogueNodeStarted;
        public System.Action<DialogueNode> OnDialogueNodeCompleted;
        public System.Action<string> OnDialogueSessionStarted;
        public System.Action<string> OnDialogueSessionEnded;
        public System.Action<DialogueHistoryEntry> OnDialogueHistoryAdded;

        void Awake()
        {
            // 单例模式
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 确保是根对象才能使用DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);

            // 初始化UI容器
            InitializeUIContainer();

            // 初始化默认UI预制体字典
            InitializeDefaultUIPrefabs();
        }

        void Start()
        {
            // 加载对话数据库
            LoadDialogueDatabase();
            
            // 初始化章节数据库
            if (chapterDatabase != null)
            {
                chapterDatabase.Initialize();
            }
        }

        /// <summary>
        /// 初始化UI容器（如果未配置则自动创建）
        /// </summary>
        private void InitializeUIContainer()
        {
            if (dialogueUIContainer == null)
            {
                GameObject containerObj = new GameObject("DialogueUIContainer");
                containerObj.transform.SetParent(transform);

                // 添加Canvas组件
                Canvas canvas = containerObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                canvas.overrideSorting = true;

                // 添加CanvasScaler
                UnityEngine.UI.CanvasScaler scaler = containerObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                // 添加GraphicRaycaster
                containerObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                // 确保EventSystem存在（UI交互必需）
                if (UnityEngine.EventSystems.EventSystem.current == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    DontDestroyOnLoad(eventSystemObj);
                }

                // 设置为UI层
                int uiLayer = LayerMask.NameToLayer("UI");
                if (uiLayer >= 0)
                {
                    containerObj.layer = uiLayer;
                }

                dialogueUIContainer = containerObj.transform;
            }
        }

        /// <summary>
        /// 初始化默认UI预制体字典
        /// </summary>
        public void InitializeDefaultUIPrefabs()
        {
            defaultUIPrefabs[DialogueDisplayMode.Story] = defaultStoryDialogueUIPrefab;
            defaultUIPrefabs[DialogueDisplayMode.Bubble] = defaultBubbleDialogueUIPrefab;
            defaultUIPrefabs[DialogueDisplayMode.Custom] = defaultChoiceDialogueUIPrefab;
        }

        /// <summary>
        /// 加载对话数据库
        /// </summary>
        public void LoadDialogueDatabase()
        {
            if (!string.IsNullOrEmpty(databaseJsonPath))
            {
                DialogueDatabaseLoader loader = new DialogueDatabaseLoader();
                dialogueDatabase = loader.LoadFromJson(databaseJsonPath);
                
                if (dialogueDatabase != null)
                {
                    dialogueDatabase.Initialize();
                }
                else
                {
                    Debug.LogError($"[DialogueSystemManager] 加载对话数据库失败：{databaseJsonPath}");
                }
            }
            else if (dialogueDatabase != null)
            {
                dialogueDatabase.Initialize();
            }
        }

        /// <summary>
        /// 开始对话会话
        /// </summary>
        public DialogueSession StartDialogue(string startNodeID, string sessionID = null)
        {
            if (dialogueDatabase == null)
            {
                Debug.LogError("[DialogueSystemManager] 对话数据库未加载！");
                return null;
            }

            DialogueNode startNode = dialogueDatabase.GetNode(startNodeID);
            if (startNode == null)
            {
                Debug.LogError($"[DialogueSystemManager] 找不到起始节点：{startNodeID}");
                return null;
            }

            // 创建会话ID
            if (string.IsNullOrEmpty(sessionID))
            {
                sessionID = $"session_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            // 创建新会话
            DialogueSession session = new DialogueSession(sessionID, startNodeID, dialogueDatabase);
            activeSessions[sessionID] = session;

            // 开始执行对话
            session.Start();
            OnDialogueSessionStarted?.Invoke(sessionID);
            
            return session;
        }

        /// <summary>
        /// 结束对话会话
        /// </summary>
        public void EndDialogue(string sessionID)
        {
            if (activeSessions.TryGetValue(sessionID, out DialogueSession session))
            {
                session.End();
                activeSessions.Remove(sessionID);
                OnDialogueSessionEnded?.Invoke(sessionID);
            }
        }

        /// <summary>
        /// 获取对话会话
        /// </summary>
        public DialogueSession GetSession(string sessionID)
        {
            activeSessions.TryGetValue(sessionID, out DialogueSession session);
            return session;
        }

        /// <summary>
        /// 获取或创建UI实例
        /// </summary>
        public IDialogueUI GetOrCreateUI(string uiInstanceID, DialogueDisplayMode displayMode, GameObject customPrefab = null)
        {
            // 如果已有实例，直接返回
            if (uiInstances.TryGetValue(uiInstanceID, out IDialogueUI existingUI))
            {
                return existingUI;
            }

            // 选择预制体
            GameObject prefab = customPrefab;
            if (prefab == null)
            {
                defaultUIPrefabs.TryGetValue(displayMode, out prefab);
                if (prefab == null && displayMode == DialogueDisplayMode.Custom)
                {
                    prefab = defaultChoiceDialogueUIPrefab;
                }
            }

            if (prefab == null)
            {
                Debug.LogError($"[DialogueSystemManager] 找不到显示模式 {displayMode} 对应的UI预制体！");
                return null;
            }

            // 实例化UI
            GameObject uiObj = Instantiate(prefab, dialogueUIContainer);
            uiObj.name = $"{displayMode}DialogueUI_{uiInstanceID}";
            
            // 确保ChoiceDialogueUI的Canvas在最上层（确保按钮可以点击）
            if (displayMode == DialogueDisplayMode.Custom)
            {
                Canvas uiCanvas = uiObj.GetComponentInParent<Canvas>();
                if (uiCanvas == null)
                {
                    uiCanvas = uiObj.GetComponent<Canvas>();
                    if (uiCanvas == null)
                    {
                        uiCanvas = uiObj.AddComponent<Canvas>();
                        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        uiCanvas.overrideSorting = true;
                        uiObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }
                }
                
                if (uiCanvas != null)
                {
                    uiCanvas.sortingOrder = 2000;
                    uiCanvas.overrideSorting = true;
                }
            }
            
            // 配置RectTransform（居中显示）
            RectTransform rectTransform = uiObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (rectTransform.sizeDelta.x == 0 || rectTransform.sizeDelta.y == 0)
                {
                    rectTransform.sizeDelta = new Vector2(400, 200);
                }
                
                if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.zero)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
            
            // 获取IDialogueUI组件
            IDialogueUI dialogueUI = uiObj.GetComponent<IDialogueUI>();
            if (dialogueUI == null)
            {
                dialogueUI = uiObj.GetComponentInChildren<IDialogueUI>();
            }

            if (dialogueUI == null)
            {
                Debug.LogError($"[DialogueSystemManager] UI预制体 '{prefab.name}' 没有实现 IDialogueUI 接口！");
                Destroy(uiObj);
                return null;
            }

            // 注册UI实例
            uiInstances[uiInstanceID] = dialogueUI;
            
            return dialogueUI;
        }

        /// <summary>
        /// 添加对话历史记录
        /// </summary>
        public void AddHistoryEntry(DialogueHistoryEntry entry)
        {
            dialogueHistory.Add(entry);
            
            // 解锁对应章节（如果entry有chapterID）
            if (!string.IsNullOrEmpty(entry.chapterID))
            {
                unlockedChapters.Add(entry.chapterID);
            }
            
            // 限制历史记录数量
            if (dialogueHistory.Count > maxHistoryEntries)
            {
                dialogueHistory.RemoveAt(0);
            }

            OnDialogueHistoryAdded?.Invoke(entry);
        }

        /// <summary>
        /// 检查章节是否已解锁
        /// </summary>
        public bool IsChapterUnlocked(string chapterID)
        {
            if (string.IsNullOrEmpty(chapterID)) return false;
            return unlockedChapters.Contains(chapterID);
        }

        /// <summary>
        /// 获取所有已解锁的章节ID
        /// </summary>
        public HashSet<string> GetUnlockedChapters()
        {
            return new HashSet<string>(unlockedChapters);
        }

        /// <summary>
        /// 获取指定章节的历史记录
        /// </summary>
        public List<DialogueHistoryEntry> GetHistoryByChapter(string chapterID)
        {
            if (string.IsNullOrEmpty(chapterID))
            {
                // 返回没有章节的历史记录
                return dialogueHistory.FindAll(e => string.IsNullOrEmpty(e.chapterID));
            }
            
            return dialogueHistory.FindAll(e => e.chapterID == chapterID);
        }

        /// <summary>
        /// 获取对话历史
        /// </summary>
        public List<DialogueHistoryEntry> GetHistory()
        {
            return new List<DialogueHistoryEntry>(dialogueHistory);
        }

        /// <summary>
        /// 清空对话历史
        /// </summary>
        public void ClearHistory()
        {
            dialogueHistory.Clear();
        }

        /// <summary>
        /// 获取活跃会话数量
        /// </summary>
        public int GetActiveSessionCount()
        {
            return activeSessions.Count;
        }

        /// <summary>
        /// 检查是否有活跃的对话会话
        /// </summary>
        public bool HasActiveSessions()
        {
            return activeSessions.Count > 0;
        }

        /// <summary>
        /// 获取所有活跃的对话会话
        /// </summary>
        public List<DialogueSession> GetActiveSessions()
        {
            return new List<DialogueSession>(activeSessions.Values);
        }

        /// <summary>
        /// 结束所有活跃的对话会话
        /// </summary>
        public void EndAllSessions()
        {
            var sessionIDs = new List<string>(activeSessions.Keys);
            foreach (var sessionID in sessionIDs)
            {
                EndDialogue(sessionID);
            }
        }

        /// <summary>
        /// 清理所有UI实例（销毁GameObject并从字典中移除）
        /// </summary>
        public void ClearAllUIInstances()
        {
            foreach (var kvp in uiInstances)
            {
                if (kvp.Value is MonoBehaviour uiBehaviour)
                {
                    if (uiBehaviour != null && uiBehaviour.gameObject != null)
                    {
                        Destroy(uiBehaviour.gameObject);
                    }
                }
            }
            
            uiInstances.Clear();
        }

        /// <summary>
        /// 移除UI实例（从字典中移除并销毁GameObject）
        /// </summary>
        public void RemoveUIInstance(string uiInstanceID)
        {
            if (uiInstances.TryGetValue(uiInstanceID, out IDialogueUI ui))
            {
                uiInstances.Remove(uiInstanceID);
                
                if (ui is MonoBehaviour uiBehaviour)
                {
                    if (uiBehaviour != null && uiBehaviour.gameObject != null)
                    {
                        Destroy(uiBehaviour.gameObject);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 对话历史记录条目
    /// </summary>
    [System.Serializable]
    public class DialogueHistoryEntry
    {
        public string sessionID;
        public string nodeID;
        public string characterID;
        public string characterName;
        public string text;
        public System.DateTime timestamp;
        
        // 图片信息
        public string backgroundImagePath;
        public List<string> insertImagePaths;
        
        // 章节信息
        public string chapterID;

        // 选择项信息（如果是玩家选择）
        public bool isPlayerChoice;
        public string choiceText;

        public DialogueHistoryEntry(DialogueNode node, CharacterData character, string sessionID)
        {
            this.sessionID = sessionID;
            this.nodeID = node != null ? node.nodeID : "unknown";
            this.characterID = node != null ? node.characterID : null;
            this.characterName = character != null ? character.characterName : "未知";
            this.text = node != null ? node.text : "";
            this.timestamp = System.DateTime.Now;
            
            // 保存图片信息
            this.backgroundImagePath = node != null ? node.backgroundImagePath : null;
            this.insertImagePaths = node != null && node.insertImagePaths != null ? new List<string>(node.insertImagePaths) : new List<string>();
            
            // 保存章节ID
            this.chapterID = node != null ? node.chapterID : null;

            // 默认不是玩家选择
            this.isPlayerChoice = false;
            this.choiceText = null;
        }

        /// <summary>
        /// 创建玩家选择项的历史记录
        /// </summary>
        public static DialogueHistoryEntry CreatePlayerChoiceEntry(DialogueChoice choice, string sessionID, string chapterID = null)
        {
            DialogueHistoryEntry entry = new DialogueHistoryEntry(null, null, sessionID);
            entry.nodeID = "choice";
            entry.characterID = "player";
            entry.characterName = "玩家";
            entry.text = choice.text;
            entry.isPlayerChoice = true;
            entry.choiceText = choice.text;
            entry.chapterID = chapterID;
            entry.timestamp = System.DateTime.Now;
            return entry;
        }
    }
}
