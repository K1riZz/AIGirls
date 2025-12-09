using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PetStateMachine))]
[RequireComponent(typeof(Animator))]
public class PetController : MonoBehaviour
{
    public PetProfileSO Profile { get; private set; }
    public PetStateMachine StateMachine { get; private set; }
    public Animator Animator { get; private set; }

    [Header("宠物配置")]
    [Tooltip("如果场景中已存在宠物，请在此处指定其配置文件")]
    public PetProfileSO initialProfile;
    public RectTransform RectTransform { get; private set; }

    [Header("UI元素")]
    [Tooltip("右键菜单 - 底部")]
    public GameObject bottomMenu;
    [Tooltip("右键菜单 - 右侧")]
    public GameObject rightMenu;
    [Tooltip("右键菜单显示超时时间（秒）")]
    public float menuTimeout = 5f;
    [Tooltip("菜单淡入淡出动画的持续时间（秒）")]
    public float menuFadeDuration = 0.2f;
    public GameObject storyModeButton; // 在Inspector中指定剧情模式按钮
    [Header("玩家输入")]
    public GameObject playerInputContainer; // 玩家输入框的容器

    [Header("小游戏系统")]
    [Tooltip("快速点击检测器（自动查找）")]
    private MonoBehaviour rapidClickDetector; // 使用MonoBehaviour作为基类，避免编译错误

    public float Affection { get; set; }
    // 模拟的可移动桌面区域
    public Rect WalkableArea { get; set; }

    // 玩家是否正在输入
    public bool IsPlayerTyping { get; set; }

    // 闲置闲聊计时器
    public float idleChatterTimer = 0f;
    public float nextChatterTime = 0f;

    private Coroutine menuTimeoutCoroutine;
    private Coroutine menuFadeCoroutine;

    private CanvasGroup bottomMenuGroup;
    private CanvasGroup rightMenuGroup;

    void Awake()
    {
        StateMachine = GetComponent<PetStateMachine>();
        Animator = GetComponent<Animator>();
        RectTransform = GetComponent<RectTransform>();
        IsPlayerTyping = false;

        // --- 自动查找UI ---
        // 根据正确的层级结构，BottomMenu 和 RightMenu 是 Pet 的子对象，所以直接在当前 transform 下查找。
        // 这样做比手动在Inspector中拖拽更健壮，不易出错。
        if (bottomMenu == null)
        {
            Transform bottomMenuTransform = transform.Find("BottomMenu");
            if (bottomMenuTransform != null) bottomMenu = bottomMenuTransform.gameObject;
            else Debug.LogError("PetController错误: 未能自动找到子对象 'BottomMenu'。请检查其名称和层级是否正确。", this);
        }
        if (rightMenu == null)
        {
            Transform rightMenuTransform = transform.Find("RightMenu");
            if (rightMenuTransform != null) rightMenu = rightMenuTransform.gameObject;
            else Debug.LogError("PetController错误: 未能自动找到子对象 'RightMenu'。请检查其名称和层级是否正确。", this);
        }

        // 自动查找或创建快速点击检测器（使用反射避免编译错误）
        FindOrCreateRapidClickDetector();

        if (bottomMenu != null)
        {
            bottomMenuGroup = bottomMenu.GetComponent<CanvasGroup>();
            if (bottomMenuGroup == null) Debug.LogError("PetController错误：BottomMenu 物体上没有找到 CanvasGroup 组件！", bottomMenu);
        }
        if (rightMenu != null)
        {
            rightMenuGroup = rightMenu.GetComponent<CanvasGroup>();
            if (rightMenuGroup == null) Debug.LogError("PetController错误：RightMenu 物体上没有找到 CanvasGroup 组件！", rightMenu);
        }

        // 自动查找剧情按钮（在BottomMenu的子对象中）
        if (storyModeButton == null && bottomMenu != null)
        {
            // 尝试查找名称包含"Story"或"剧情"的按钮
            Transform storyBtnTransform = bottomMenu.transform.Find("StoryModeButton");
            if (storyBtnTransform == null)
            {
                storyBtnTransform = bottomMenu.transform.Find("剧情按钮");
            }
            if (storyBtnTransform == null)
            {
                // 尝试在所有子对象中查找
                foreach (Transform child in bottomMenu.transform)
                {
                    if (child.name.Contains("Story") || child.name.Contains("剧情"))
                    {
                        storyBtnTransform = child;
                        break;
                    }
                }
            }
            if (storyBtnTransform != null)
            {
                storyModeButton = storyBtnTransform.gameObject;
                Debug.Log($"[PetController] 自动找到了剧情按钮: {storyBtnTransform.name}");
            }
            else
            {
                Debug.LogWarning("[PetController] 未找到剧情按钮，请在BottomMenu下创建名为 'StoryModeButton' 或包含 'Story'/'剧情' 的按钮", this);
            }
        }
    }

    void Start()
    {
        // 如果Profile尚未初始化（意味着它不是由PetManager动态生成的），
        // 则使用在Inspector中指定的initialProfile进行初始化。
        if (Profile == null && initialProfile != null)
        {
            Initialize(initialProfile);
        }

        // 订阅新对话系统事件，以便在对话开始时强制进入idle状态（使用反射避免编译错误）
        SubscribeToDialogueEvents();
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        UnsubscribeFromDialogueEvents();
    }

    // 当前对话会话ID（用于跟踪对话）
    private string currentDialogueSessionID;

    /// <summary>
    /// 订阅对话系统事件（新对话系统，使用反射避免编译错误）
    /// </summary>
    private void SubscribeToDialogueEvents()
    {
        try
        {
            var dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            if (dialogueSystemManagerType != null)
            {
                var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        // 订阅对话会话开始事件
                        var sessionStartedEvent = dialogueSystemManagerType.GetEvent("OnDialogueSessionStarted");
                        if (sessionStartedEvent != null)
                        {
                            var handlerType = sessionStartedEvent.EventHandlerType;
                            var handler = System.Delegate.CreateDelegate(handlerType, this, "OnDialogueSessionStarted");
                            sessionStartedEvent.AddEventHandler(instance, handler);
                        }

                        // 订阅对话会话结束事件
                        var sessionEndedEvent = dialogueSystemManagerType.GetEvent("OnDialogueSessionEnded");
                        if (sessionEndedEvent != null)
                        {
                            var handlerType = sessionEndedEvent.EventHandlerType;
                            var handler = System.Delegate.CreateDelegate(handlerType, this, "OnDialogueSessionEnded");
                            sessionEndedEvent.AddEventHandler(instance, handler);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PetController] 订阅对话系统事件时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 取消订阅对话系统事件（新对话系统，使用反射避免编译错误）
    /// </summary>
    private void UnsubscribeFromDialogueEvents()
    {
        try
        {
            var dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            if (dialogueSystemManagerType != null)
            {
                var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        // 取消订阅对话会话开始事件
                        var sessionStartedEvent = dialogueSystemManagerType.GetEvent("OnDialogueSessionStarted");
                        if (sessionStartedEvent != null)
                        {
                            var handlerType = sessionStartedEvent.EventHandlerType;
                            var handler = System.Delegate.CreateDelegate(handlerType, this, "OnDialogueSessionStarted");
                            sessionStartedEvent.RemoveEventHandler(instance, handler);
                        }

                        // 取消订阅对话会话结束事件
                        var sessionEndedEvent = dialogueSystemManagerType.GetEvent("OnDialogueSessionEnded");
                        if (sessionEndedEvent != null)
                        {
                            var handlerType = sessionEndedEvent.EventHandlerType;
                            var handler = System.Delegate.CreateDelegate(handlerType, this, "OnDialogueSessionEnded");
                            sessionEndedEvent.RemoveEventHandler(instance, handler);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PetController] 取消订阅对话系统事件时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 对话会话开始事件处理（新对话系统）
    /// </summary>
    private void OnDialogueSessionStarted(string sessionID)
    {
        currentDialogueSessionID = sessionID;
        Debug.Log("[PetController] 对话会话开始，强制进入idle状态");
        ForceIdleState();
    }

    /// <summary>
    /// 对话会话结束事件处理（新对话系统）
    /// </summary>
    private void OnDialogueSessionEnded(string sessionID)
    {
        if (currentDialogueSessionID == sessionID)
        {
            currentDialogueSessionID = null;
        }
        Debug.Log("[PetController] 对话会话结束");
    }

    /// <summary>
    /// 查找或创建快速点击检测器（使用反射避免编译错误）
    /// </summary>
    private void FindOrCreateRapidClickDetector()
    {
        try
        {
            var rapidClickDetectorType = System.Type.GetType("RapidClickDetector");
            if (rapidClickDetectorType != null)
            {
                // 尝试在子对象中查找
                MonoBehaviour found = GetComponentInChildren(rapidClickDetectorType) as MonoBehaviour;
                if (found != null)
                {
                    rapidClickDetector = found;
                    return;
                }

                // 如果没有找到，尝试在Canvas子对象上创建
                Transform canvasTransform = transform.Find("Canvas");
                if (canvasTransform != null)
                {
                    rapidClickDetector = canvasTransform.gameObject.AddComponent(rapidClickDetectorType) as MonoBehaviour;
                }
                else
                {
                    Debug.LogWarning("PetController: 未找到Canvas子对象，无法创建RapidClickDetector。请手动添加。", this);
                }
            }
        }
        catch
        {
            // RapidClickDetector 可能尚未编译，忽略错误
            Debug.LogWarning("PetController: RapidClickDetector类型未找到，快速点击检测功能将不可用。", this);
        }
    }

    public void Initialize(PetProfileSO profile)
    {
        this.Profile = profile;
        this.name = $"Pet_{profile.petName}";

        // 初始化好感度等数值（从存档中读取，或使用默认值）
        Affection = PlayerPrefs.GetFloat("PetAffection", 0f);

        // 初始化状态机
        StateMachine.Initialize(this);

        // 默认隐藏所有交互UI
        // if (storyModeButton != null) storyModeButton.SetActive(false); // 由ShowContextMenus动态控制
        if (playerInputContainer != null) playerInputContainer.SetActive(false); 

        // 初始化菜单CanvasGroup状态
        if (bottomMenuGroup != null) {
            bottomMenuGroup.alpha = 0;
            bottomMenuGroup.interactable = false;
        }
        if (rightMenuGroup != null) {
            rightMenuGroup.alpha = 0;
            rightMenuGroup.interactable = false;
        }

        // 初始化闲聊计时器
        ResetIdleChatterTimer();
    }

    /// <summary>
    /// 重置闲置闲聊计时器，并设置下一次闲聊的随机时间。
    /// </summary>
    public void ResetIdleChatterTimer()
    {
        idleChatterTimer = 0f;
        nextChatterTime = Random.Range(Profile.idleChatterIntervalMin, Profile.idleChatterIntervalMax);
        Debug.Log($"[PetController] 下次闲聊将在 {nextChatterTime} 秒后。");
    }

    /// <summary>
    /// 根据当前的运行环境（编辑器或打包后的程序）更新宠物的可活动桌面范围。
    /// 这个方法应该在每次进入桌面模式时调用，以确保活动范围的准确性。
    /// </summary>
    public void UpdateWalkableArea()
    {
        float screenWidth;
        float screenHeight;

#if UNITY_EDITOR
        // 在编辑器中，使用主摄像机的像素尺寸来精确计算活动范围，以获得最准确的预览
        // 这能确保宠物始终在Game窗口内活动
        screenWidth = Camera.main.pixelWidth;
        screenHeight = Camera.main.pixelHeight;
#else
        // 在打包后的程序中，我们使用整个显示器的分辨率
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;
#endif

        var petRect = RectTransform.rect;
        float halfWidth = petRect.width / 2;
        float halfHeight = petRect.height / 2;

        WalkableArea = new Rect(
            halfWidth, halfHeight,
            screenWidth - petRect.width, screenHeight - petRect.height);
        Debug.Log($"[PetController] 更新活动范围为: {WalkableArea}");
    }

    // 由PetInteraction调用
    public void OnClicked()
    {
        if (Profile == null)
        {
            Debug.LogError("[PetController] Profile引用失败.", this);
            return;
        }

        Debug.Log($"[PetController] 左键点击，touchDialogueNodeID: {Profile.touchDialogueNodeID}");

        // 如果右键菜单是打开的，则左键点击任何地方（包括宠物自己）都应关闭菜单
        if (bottomMenuGroup != null && bottomMenuGroup.alpha > 0)
        {
            Debug.Log("[PetController] 菜单打开中，关闭菜单");
            HideContextMenus();
            return;
        }

        // 检查是否在小游戏中，如果是则只记录点击，不触发对话（使用反射避免编译错误）
        try
        {
            var miniGameManagerType = System.Type.GetType("MiniGameManager");
            if (miniGameManagerType != null)
            {
                var instanceProperty = miniGameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var isActiveProperty = miniGameManagerType.GetProperty("IsMiniGameActive");
                        if (isActiveProperty != null)
                        {
                            bool isActive = (bool)isActiveProperty.GetValue(instance);
                            if (isActive)
                            {
                                Debug.Log("[PetController] 当前在小游戏中，忽略点击");
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // MiniGameManager 可能尚未编译，忽略错误
        }

        // 检查是否有对话节点ID配置
        if (string.IsNullOrEmpty(Profile.touchDialogueNodeID))
        {
            Debug.LogWarning("[PetController] Profile.touchDialogueNodeID 未配置，无法触发对话");
            return;
        }

        // 检查对话是否正在进行
        if (IsDialogueActive())
        {
            Debug.Log("[PetController] 对话正在进行中，忽略点击");
            return;
        }

        // 记录点击用于快速点击检测（延迟触发Bark，避免快速点击时触发Bark）
        bool shouldTriggerBark = true;
        if (rapidClickDetector != null)
        {
            // 记录点击，但不立即触发Bark（使用反射调用）
            // 如果快速点击检测成功，将在OnRapidClickDetected中阻止Bark
            try
            {
                var registerMethod = rapidClickDetector.GetType().GetMethod("RegisterClick");
                if (registerMethod != null)
                {
                    registerMethod.Invoke(rapidClickDetector, null);
                    
                    // 延迟一小段时间再触发对话，给快速点击检测时间判断
                    Debug.Log("[PetController] 使用快速点击检测器，延迟触发对话");
                    StartCoroutine(DelayedBarkCheck(Profile.touchDialogueNodeID, Profile.touchConversationDuration));
                    shouldTriggerBark = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PetController] 快速点击检测器调用失败: {e.Message}");
                // 如果调用失败，继续使用默认行为
            }
        }

        // 如果没有快速点击检测器，直接触发对话（使用新对话系统）
        if (shouldTriggerBark)
        {
            Debug.Log($"[PetController] 直接触发对话: {Profile.touchDialogueNodeID}");
            StartDialogue(Profile.touchDialogueNodeID);
        }
    }

    // 由PetInteraction调用
    public void OnRightClicked()
    {
        bool isMenuClosed = (bottomMenuGroup != null && bottomMenuGroup.alpha < 1);
        Debug.Log(bottomMenuGroup != null ? "有的": "没有");
        
        if (isMenuClosed)
        {
            ShowContextMenus();
        }
        else
        {
            HideContextMenus();
        }  
    }

    private void ShowContextMenus()
    {
        // 显示菜单时，强制进入idle状态
        ForceIdleState();

        // 如果还没找到剧情按钮，再次尝试查找
        if (storyModeButton == null && bottomMenu != null)
        {
            Transform storyBtnTransform = bottomMenu.transform.Find("StoryModeButton");
            if (storyBtnTransform == null)
            {
                storyBtnTransform = bottomMenu.transform.Find("剧情按钮");
            }
            if (storyBtnTransform == null)
            {
                foreach (Transform child in bottomMenu.transform)
                {
                    if (child.name.Contains("Story") || child.name.Contains("剧情"))
                    {
                        storyBtnTransform = child;
                        break;
                    }
                }
            }
            if (storyBtnTransform != null)
            {
                storyModeButton = storyBtnTransform.gameObject;
                Debug.Log($"[PetController] ShowContextMenus: 找到剧情按钮: {storyBtnTransform.name}");
            }
        }

        // 根据Profile中是否存在剧情对话来决定是否显示"剧情"按钮
        // 注意：在菜单淡入之前设置按钮状态，确保按钮正确显示
        if (storyModeButton != null)
        {
            bool isStoryAvailable = (Profile != null && !string.IsNullOrEmpty(Profile.startDialogueNodeID));
            storyModeButton.SetActive(isStoryAvailable);
            Debug.Log($"[PetController] 剧情按钮状态: {(isStoryAvailable ? "显示" : "隐藏")}, startDialogueNodeID: {Profile?.startDialogueNodeID}, 按钮GameObject: {storyModeButton.name}");
        }
        else
        {
            Debug.LogWarning("[PetController] ShowContextMenus: storyModeButton 为 null，无法显示剧情按钮");
        }

        // 确保菜单GameObject是激活的
        if (bottomMenu != null && !bottomMenu.activeSelf)
        {
            bottomMenu.SetActive(true);
        }

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine);
        menuFadeCoroutine = StartCoroutine(FadeMenus(true));

        // 开始超时隐藏计时
        if (menuTimeoutCoroutine != null) StopCoroutine(menuTimeoutCoroutine);
        menuTimeoutCoroutine = StartCoroutine(MenuTimeoutCoroutine());
    }

    public void HideContextMenus(bool immediate = false)
    {
        // 停止超时计时
        if (menuTimeoutCoroutine != null)
        {
            StopCoroutine(menuTimeoutCoroutine);
            menuTimeoutCoroutine = null;
        }

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine);
        if (immediate || gameObject.activeInHierarchy == false)
        {
            if (bottomMenuGroup != null) bottomMenuGroup.alpha = 0;
            if (rightMenuGroup != null) rightMenuGroup.alpha = 0;
            if (bottomMenuGroup != null) bottomMenuGroup.interactable = false;
            if (rightMenuGroup != null) rightMenuGroup.interactable = false;
        }
        else
        {
            menuFadeCoroutine = StartCoroutine(FadeMenus(false));
        }
    }

    private IEnumerator MenuTimeoutCoroutine()
    {
        yield return new WaitForSeconds(menuTimeout);
        Debug.Log("菜单超时，自动隐藏。");
        HideContextMenus();
        menuTimeoutCoroutine = null;
    }

    private IEnumerator FadeMenus(bool fadeIn)
    {
        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;
        float timer = 0f;

        if (fadeIn)
        {
            if (bottomMenuGroup != null) bottomMenuGroup.interactable = true;
            if (rightMenuGroup != null) rightMenuGroup.interactable = true;
        }

        while (timer < menuFadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / menuFadeDuration);
            if (bottomMenuGroup != null) bottomMenuGroup.alpha = alpha;
            if (rightMenuGroup != null) rightMenuGroup.alpha = alpha;
            yield return null;
        }

        if (!fadeIn)
        {
            if (bottomMenuGroup != null) bottomMenuGroup.interactable = false;
            if (rightMenuGroup != null) rightMenuGroup.interactable = false;
        }
        menuFadeCoroutine = null;
    }

    /// <summary>
    /// 触发一个气泡对话（使用新对话系统）。
    /// </summary>
    /// <param name="nodeID">对话节点ID</param>
    /// <param name="duration">显示时长（秒，已废弃，由节点配置控制）</param>
    public void TriggerBark(string nodeID, float duration)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            return;
        }
        
        // 检查对话是否正在进行
        if (IsDialogueActive())
        {
            return;
        }
        
        // 使用新对话系统触发气泡对话
        StartDialogue(nodeID);
    }

    /// <summary>
    /// 检查对话是否正在进行（新对话系统）
    /// </summary>
    private bool IsDialogueActive()
    {
        // 先检查当前会话ID
        if (!string.IsNullOrEmpty(currentDialogueSessionID))
        {
            // 检查会话是否仍然活跃（使用反射，避免编译错误）
            try
            {
                var dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
                if (dialogueSystemManagerType != null)
                {
                    var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        var instance = instanceProperty.GetValue(null);
                        if (instance != null)
                        {
                            var getSessionMethod = dialogueSystemManagerType.GetMethod("GetSession", new System.Type[] { typeof(string) });
                            if (getSessionMethod != null)
                            {
                                var session = getSessionMethod.Invoke(instance, new object[] { currentDialogueSessionID });
                                if (session != null)
                                {
                                    // 检查会话是否活跃
                                    var isActiveProperty = session.GetType().GetProperty("isActive");
                                    if (isActiveProperty != null)
                                    {
                                        return (bool)isActiveProperty.GetValue(session);
                                    }
                                    return true; // 如果找不到isActive属性，假设会话存在就是活跃的
                                }
                            }
                            
                            // 或者使用HasActiveSessions方法
                            var hasActiveMethod = dialogueSystemManagerType.GetMethod("HasActiveSessions");
                            if (hasActiveMethod != null)
                            {
                                return (bool)hasActiveMethod.Invoke(instance, null);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PetController] 检查对话状态时发生错误: {e.Message}");
            }
        }
        
        // 使用更通用的方法检查是否有任何活跃会话
        try
        {
            var dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            if (dialogueSystemManagerType != null)
            {
                var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var hasActiveMethod = dialogueSystemManagerType.GetMethod("HasActiveSessions");
                        if (hasActiveMethod != null)
                        {
                            return (bool)hasActiveMethod.Invoke(instance, null);
                        }
                    }
                }
            }
        }
        catch
        {
            // 新对话系统可能尚未编译，返回false
        }
        
        return false;
    }

    /// <summary>
    /// 开始对话（新对话系统，使用反射避免编译错误）
    /// </summary>
    private void StartDialogue(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            Debug.LogWarning("[PetController] StartDialogue: nodeID 为空");
            return;
        }

        try
        {
            // 尝试多种方式查找类型
            System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            
            // 如果找不到，尝试从所有已加载的程序集中查找
            if (dialogueSystemManagerType == null)
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                    if (dialogueSystemManagerType != null)
                    {
                        break;
                    }
                }
            }
            
            if (dialogueSystemManagerType == null)
            {
                Debug.LogError("[PetController] 无法找到 NewDialogueSystem.DialogueSystemManager 类型！");
                return;
            }

            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogError("[PetController] DialogueSystemManager 没有 Instance 属性");
                return;
            }

            var instance = instanceProperty.GetValue(null);
            if (instance == null)
            {
                Debug.LogError("[PetController] DialogueSystemManager.Instance 为 null");
                return;
            }

            // 尝试查找StartDialogue方法（可能有一个或两个参数）
            System.Reflection.MethodInfo startMethod = null;
            
            // 先尝试查找两个参数的方法（startNodeID, sessionID）
            startMethod = dialogueSystemManagerType.GetMethod("StartDialogue", new System.Type[] { typeof(string), typeof(string) });
            if (startMethod == null)
            {
                // 尝试只带一个参数的方法
                startMethod = dialogueSystemManagerType.GetMethod("StartDialogue", new System.Type[] { typeof(string) });
            }

            if (startMethod == null)
            {
                Debug.LogError("[PetController] DialogueSystemManager 没有 StartDialogue 方法");
                return;
            }

            // 调用StartDialogue方法
            // 根据方法签名决定参数：如果方法需要2个参数，第二个为sessionID（可选）
            object session;
            var methodParams = startMethod.GetParameters();
            if (methodParams.Length == 1)
            {
                session = startMethod.Invoke(instance, new object[] { nodeID });
            }
            else if (methodParams.Length == 2)
            {
                session = startMethod.Invoke(instance, new object[] { nodeID, null }); // sessionID为null时自动生成
            }
            else
            {
                Debug.LogError($"[PetController] StartDialogue方法参数数量不正确: {methodParams.Length}");
                return;
            }
            
            if (session != null)
            {
                // 设置对话UI的目标位置为宠物位置（用于气泡对话跟随）
                var sessionType = session.GetType();
                var setTargetMethod = sessionType.GetMethod("SetTargetTransform");
                Debug.Log($"[PetController] 查找 SetTargetTransform 方法: {(setTargetMethod != null ? "成功" : "失败")}");
                if (setTargetMethod != null)
                {
                    setTargetMethod.Invoke(session, new object[] { this.transform });
                    Debug.Log($"[PetController] 已调用 SetTargetTransform，目标: {this.name}");
                }
                else
                {
                    Debug.LogWarning("[PetController] 未找到 SetTargetTransform 方法");
                }
                
                // 获取会话ID
                var sessionIDProperty = sessionType.GetProperty("sessionID");
                if (sessionIDProperty != null)
                {
                    currentDialogueSessionID = (string)sessionIDProperty.GetValue(session);
                }
            }
            else
            {
                Debug.LogError("[PetController] 启动对话失败");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PetController] 启动对话时发生错误: {e.Message}\n{e.StackTrace}");
        }
    }
    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }

    #region 右键菜单按钮功能

    /// <summary>
    /// 进入剧情模式（由剧情按钮调用）
    /// </summary>
    public void EnterStoryMode()
    {
        HideContextMenus();
        Debug.Log("[PetController] 进入剧情模式...");
        
        // 停止当前的所有活动
        ForceIdleState();

        // 使用StoryModeManager进入剧情模式（使用反射避免编译错误）
        try
        {
            var storyModeManagerType = System.Type.GetType("StoryModeManager");
            if (storyModeManagerType != null)
            {
                var instanceProperty = storyModeManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var enterMethod = storyModeManagerType.GetMethod("EnterStoryMode", new System.Type[] { typeof(string) });
                        if (enterMethod != null)
                        {
                            enterMethod.Invoke(instance, new object[] { Profile.startDialogueNodeID });
                            return;
                        }
                    }
                }
            }
        }
        catch
        {
            // StoryModeManager 可能尚未编译，使用备用方案
        }
        
        // 备用方案：直接触发对话（使用新对话系统）
        Debug.LogWarning("[PetController] StoryModeManager未找到，使用备用方案直接触发对话");
        if (!string.IsNullOrEmpty(Profile.startDialogueNodeID))
        {
            StartDialogue(Profile.startDialogueNodeID);
        }
    }

    public void TogglePlayerInput()
    {
        HideContextMenus();
        if (playerInputContainer != null)
        {
            bool isVisible = !playerInputContainer.activeSelf;
            playerInputContainer.SetActive(isVisible);
            Debug.Log(isVisible ? "显示对话框" : "隐藏对话框");
            // 如果显示输入框，强制进入idle状态
            if (isVisible)
            {
                ForceIdleState();
            }
        }
    }

    public void ReturnToRoom()
    {
        HideContextMenus();
        Debug.Log("（预留功能）回到房间");
    }

    public void OpenCollectibles()
    {
        HideContextMenus();
        Debug.Log("（预留功能）打开收藏品");
    }

    public void TakeScreenshot()
    {
        HideContextMenus();
        Debug.Log("（预留功能）截图");
    }

    public void CreateOnlineRoom()
    {
        HideContextMenus();
        Debug.Log("（预留功能）创建联机房间");
    }

    public void ShowSettings()
    {
        HideContextMenus();
        Debug.Log("（预留功能）显示设置");
    }

    public void ExitGame()
    {
        HideContextMenus();
        Debug.Log("退出游戏...");
        Application.Quit();
    }
    #endregion

    /// <summary>
    /// 停止宠物的移动，强制切换到Idle状态。
    /// </summary>
    public void StopMovement()
    {
        if (StateMachine.CurrentState is WanderState)
        {
            StateMachine.SwitchState(new IdleState(this));
        }
    }

    /// <summary>
    /// 检查菜单是否可见（alpha > 0）
    /// </summary>
    public bool IsMenuVisible()
    {
        return (bottomMenuGroup != null && bottomMenuGroup.alpha > 0) || 
               (rightMenuGroup != null && rightMenuGroup.alpha > 0);
    }

    /// <summary>
    /// 检查是否应该强制保持idle状态。
    /// 当对话激活、菜单显示或输入框激活时，应该保持idle状态。
    /// </summary>
    public bool ShouldForceIdle()
    {
        // 检查对话是否激活（新对话系统）
        if (IsDialogueActive())
        {
            return true;
        }

        // 检查菜单是否显示（alpha > 0 表示菜单可见）
        if (IsMenuVisible())
        {
            return true;
        }

        // 检查输入框是否激活
        if (playerInputContainer != null && playerInputContainer.activeSelf)
        {
            return true;
        }

        // 检查是否在小游戏中（使用反射避免编译错误）
        try
        {
            var miniGameManagerType = System.Type.GetType("MiniGameManager");
            if (miniGameManagerType != null)
            {
                var instanceProperty = miniGameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var isActiveProperty = miniGameManagerType.GetProperty("IsMiniGameActive");
                        if (isActiveProperty != null)
                        {
                            bool isActive = (bool)isActiveProperty.GetValue(instance);
                            if (isActive)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // MiniGameManager 可能尚未编译，忽略错误
        }

        return false;
    }

    /// <summary>
    /// 强制切换到Idle状态（如果当前不是Idle状态）。
    /// </summary>
    public void ForceIdleState()
    {
        if (!(StateMachine.CurrentState is IdleState))
        {
            StateMachine.SwitchState(new IdleState(this));
        }
    }

    
    /// <summary>
    /// 显示玩家输入的内容作为一个Bark气泡。
    /// </summary>
    /// <param name="message">玩家输入的消息</param>
    public void ShowPlayerBark(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        // 如果宠物正在移动，则强制切换到Idle状态
        if (StateMachine.CurrentState is WanderState)
        {
            StateMachine.SwitchState(new IdleState(this));
        }

        // 使用新对话系统显示玩家输入（可以通过创建一个临时的气泡对话节点来实现）
        // 或者直接使用BubbleDialogueUI显示文本
        // TODO: 实现直接显示玩家输入的功能
        Debug.Log($"[PetController] 玩家输入: {message}");

        Debug.Log($"[PetController] 显示玩家输入: '{message}'");
        // 重置闲聊计时器，避免立即触发闲聊
        ResetIdleChatterTimer();
    }

    /// <summary>
    /// 安全地隐藏当前正在显示的任何对话气泡（已废弃，由新对话系统管理）
    /// </summary>
    public void SafelyHideCurrentBark()
    {
        // 新对话系统会自动管理对话的显示和隐藏，此方法已废弃
        // 如果需要强制结束对话，可以使用：
        // if (DialogueSystemManager.Instance != null && !string.IsNullOrEmpty(currentDialogueSessionID))
        // {
        //     DialogueSystemManager.Instance.EndDialogue(currentDialogueSessionID);
        // }
    }

    /// <summary>
    /// 延迟检查是否触发Bark（给快速点击检测时间）
    /// </summary>
    private System.Collections.IEnumerator DelayedBarkCheck(string conversationTitle, float duration)
    {
        yield return new WaitForSeconds(0.1f); // 延迟0.1秒

        // 如果在这期间触发了小游戏，则不触发对话（使用反射避免编译错误）
        try
        {
            var miniGameManagerType = System.Type.GetType("MiniGameManager");
            if (miniGameManagerType != null)
            {
                var instanceProperty = miniGameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var isActiveProperty = miniGameManagerType.GetProperty("IsMiniGameActive");
                        if (isActiveProperty != null)
                        {
                            bool isActive = (bool)isActiveProperty.GetValue(instance);
                            if (isActive)
                            {
                                Debug.Log("[PetController] 快速点击检测成功，取消对话触发");
                                yield break;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // MiniGameManager 可能尚未编译，忽略错误
        }

        // 如果不在小游戏中，且没有对话正在进行，则触发对话（新对话系统）
        if (!string.IsNullOrEmpty(conversationTitle) && !IsDialogueActive())
        {
            TriggerBark(conversationTitle, duration);
        }
    }


}