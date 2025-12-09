using UnityEngine;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PetProfileSO currentPetProfile; // 在Inspector中指定默认的宠物

    private float hourlyCheckTimer = 0f;
    private const float HOURLY_CHECK_INTERVAL = 3600f; // 每小时检查一次

    void Awake()
    {
        Debug.Log("[GameManager] Awake() 开始执行");
        
        if (Instance != null)
        {
            Debug.LogWarning($"[GameManager] 检测到重复的 GameManager，销毁当前对象：{gameObject.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log($"[GameManager] Instance 已设置，GameObject名称: {gameObject.name}，场景: {gameObject.scene.name}，激活状态: {gameObject.activeSelf}");
        Debug.Log("[GameManager] 开始初始化组件...");

        // 添加Windows控制器，用于处理桌面化逻辑
        // 确保我们添加的是正确的、唯一的WindowsController
        if (GetComponent<WindowsController>() == null) {
            gameObject.AddComponent<WindowsController>();
        }
        
        // 添加桌面输入追踪器，用于记录鼠标键盘次数
        if (GetComponent<DesktopInputTracker>() == null)
        {
            gameObject.AddComponent<DesktopInputTracker>();
        }

        // 初始化桌面图标控制器
        InitializeDesktopIconController();

        // 初始化剧情模式管理器
        InitializeStoryModeManager();

        // 初始化小游戏管理器
        InitializeMiniGameManager();

        // 初始化新对话系统
        InitializeDialogueSystem();
        
        Debug.Log("[GameManager] Awake() 完成");
    }

    void Start()
    {
        Debug.Log("[GameManager] Start() 开始执行");
        
        // 游戏开始时，根据当前选择的Profile生成宠物
        // 确保PetManager实例存在
        if (currentPetProfile != null && PetManager.Instance != null)
        {
            PetManager.Instance.SpawnPet(currentPetProfile);
            
            // 宠物生成后，立即为它设置初始的活动范围
            if (PetManager.Instance.ActivePet != null)
            {
                PetManager.Instance.ActivePet.UpdateWalkableArea();
            }
        }

        // 注册小游戏
        RegisterMiniGames();
        
        // 延迟配置对话系统（确保所有组件都已初始化）
        StartCoroutine(ConfigureDialogueSystemDelayed());
        
        Debug.Log("[GameManager] Start() 完成");
    }

    /// <summary>
    /// 初始化桌面图标控制器（使用反射避免编译错误）
    /// </summary>
    private void InitializeDesktopIconController()
    {
        try
        {
            var desktopIconControllerType = System.Type.GetType("DesktopIconController");
            if (desktopIconControllerType != null)
            {
                var instanceProperty = desktopIconControllerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance == null)
                    {
                        GameObject iconControllerObj = new GameObject("DesktopIconController");
                        iconControllerObj.transform.SetParent(transform);
                        iconControllerObj.AddComponent(desktopIconControllerType);
                        Debug.Log("[GameManager] 创建了DesktopIconController");
                    }
                    else
                    {
                        Debug.Log("[GameManager] DesktopIconController已存在");
                    }
                }
            }
        }
        catch
        {
            // DesktopIconController 可能尚未编译，忽略错误
        }
    }

    /// <summary>
    /// 初始化剧情模式管理器（使用反射避免编译错误）
    /// </summary>
    private void InitializeStoryModeManager()
    {
        // 初始化StoryModeManager
        try
        {
            var storyModeManagerType = System.Type.GetType("StoryModeManager");
            if (storyModeManagerType != null)
            {
                var instanceProperty = storyModeManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance == null)
                    {
                        GameObject storyModeObj = new GameObject("StoryModeManager");
                        storyModeObj.transform.SetParent(transform);
                        storyModeObj.AddComponent(storyModeManagerType);
                        Debug.Log("[GameManager] 创建了StoryModeManager");
                    }
                    else
                    {
                        Debug.Log("[GameManager] StoryModeManager已存在");
                    }
                }
            }
        }
        catch
        {
            // StoryModeManager 可能尚未编译，忽略错误
        }

        // 初始化StoryModeUI
        try
        {
            var storyModeUIType = System.Type.GetType("StoryModeUI");
            if (storyModeUIType != null)
            {
                var instanceProperty = storyModeUIType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance == null)
                    {
                        GameObject storyUIObj = new GameObject("StoryModeUI");
                        storyUIObj.transform.SetParent(transform);
                        storyUIObj.AddComponent(storyModeUIType);
                        Debug.Log("[GameManager] 创建了StoryModeUI");
                    }
                    else
                    {
                        Debug.Log("[GameManager] StoryModeUI已存在");
                    }
                }
            }
        }
        catch
        {
            // StoryModeUI 可能尚未编译，忽略错误
        }
    }

    /// <summary>
    /// 初始化小游戏管理器（使用反射避免编译错误）
    /// </summary>
    private void InitializeMiniGameManager()
    {
        try
        {
            var miniGameManagerType = System.Type.GetType("MiniGameManager");
            if (miniGameManagerType != null)
            {
                var instanceProperty = miniGameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance == null)
                    {
                        GameObject managerObj = new GameObject("MiniGameManager");
                        managerObj.transform.SetParent(transform);
                        managerObj.AddComponent(miniGameManagerType);
                        Debug.Log("[GameManager] 创建了MiniGameManager作为子对象");
                    }
                    else
                    {
                        Debug.Log("[GameManager] MiniGameManager已存在");
                    }
                }
            }
        }
        catch
        {
            // MiniGameManager 可能尚未编译，忽略错误
        }
    }

    /// <summary>
    /// 注册所有小游戏（使用反射避免编译错误）
    /// </summary>
    private void RegisterMiniGames()
    {
        // 等待一帧确保MiniGameManager的Start已经执行（UI容器已创建）
        StartCoroutine(RegisterMiniGamesCoroutine());
    }

    private System.Collections.IEnumerator RegisterMiniGamesCoroutine()
    {
        yield return null; // 等待一帧

        // 将 try-catch 移到 yield return 之后
        RegisterMiniGamesInternal();
    }

    /// <summary>
    /// 注册小游戏的内部逻辑（不在协程中，可以安全使用 try-catch）
    /// </summary>
    private void RegisterMiniGamesInternal()
    {
        try
        {
            var miniGameManagerType = System.Type.GetType("MiniGameManager");
            if (miniGameManagerType == null) return;

            var instanceProperty = miniGameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null) return;

            var instance = instanceProperty.GetValue(null);
            if (instance == null) return;

            // 创建并注册泡泡消除小游戏
            var bubblePopMiniGameType = System.Type.GetType("BubblePopMiniGame");
            if (bubblePopMiniGameType != null)
            {
                GameObject bubblePopObj = new GameObject("BubblePopMiniGame");
                var miniGameManagerTransform = (instance as MonoBehaviour)?.transform;
                if (miniGameManagerTransform != null)
                {
                    bubblePopObj.transform.SetParent(miniGameManagerTransform);
                }

                var bubblePopMiniGame = bubblePopObj.AddComponent(bubblePopMiniGameType);

                // 调用RegisterMiniGame方法
                var registerMethod = miniGameManagerType.GetMethod("RegisterMiniGame");
                if (registerMethod != null)
                {
                    registerMethod.Invoke(instance, new object[] { bubblePopMiniGame });
                    Debug.Log("[GameManager] 注册了BubblePopMiniGame");
                }
            }
        }
        catch
        {
            // MiniGameManager 或 BubblePopMiniGame 可能尚未编译，忽略错误
        }
    }

    void OnDestroy()
    {
        // 取消订阅新对话系统事件（使用反射）
        UnsubscribeFromDialogueEvents();
    }

    void Update()
    {
        // 每小时检查一次
        hourlyCheckTimer += Time.deltaTime;
        if (hourlyCheckTimer >= HOURLY_CHECK_INTERVAL)
        {
            hourlyCheckTimer = 0;
            CheckTimeBasedEvents();
        }
    }

    void CheckTimeBasedEvents()
    {
        // 检查当前是否是特定时间点，例如整点
        DateTime now = DateTime.Now;
        if (now.Minute == 0 && currentPetProfile != null && !string.IsNullOrEmpty(currentPetProfile.startDialogueNodeID)) // 如果是整点
        {
            // 触发定时对话（需要在新对话数据库中配置对应的节点ID）
            // TODO: 在对话数据库中配置定时对话节点ID
            // StartDialogueViaReflection("hourly_chime_01");
        }
    }

    public void SaveGame()
    {
        // 保存游戏数据（包括好感度等）
        if (PetManager.Instance != null && PetManager.Instance.ActivePet != null)
        {
            PlayerPrefs.SetFloat("PetAffection", PetManager.Instance.ActivePet.Affection);
        }
        PlayerPrefs.Save();
        Debug.Log("游戏已保存!");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("PetAffection") && PetManager.Instance != null && PetManager.Instance.ActivePet != null)
        {
            PetManager.Instance.ActivePet.Affection = PlayerPrefs.GetFloat("PetAffection");
        }
        Debug.Log("游戏已加载!");
    }

    /// <summary>
    /// 初始化新对话系统（使用反射避免编译错误）
    /// </summary>
    private void InitializeDialogueSystem()
    {
        Debug.Log("[GameManager] 开始初始化对话系统...");
        
        // 首先检查场景中是否已经存在DialogueSystemManager GameObject
        GameObject existingObj = GameObject.Find("DialogueSystemManager");
        if (existingObj != null)
        {
            Debug.Log("[GameManager] 场景中已存在 DialogueSystemManager GameObject");
            // 订阅对话结束事件，用于从剧情模式切换回桌面模式（延迟到下一帧，确保Instance已设置）
            StartCoroutine(SubscribeToDialogueEventsDelayed());
            return;
        }
        
        // 确保DialogueSystemManager存在（使用反射）
        try
        {
            // 尝试多种方式查找类型
            System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            
            // 如果找不到，尝试从所有已加载的程序集中查找
            if (dialogueSystemManagerType == null)
            {
                Debug.Log("[GameManager] 直接查找类型失败，尝试从所有程序集中查找...");
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                    if (dialogueSystemManagerType != null)
                    {
                        Debug.Log($"[GameManager] 从程序集 {assembly.FullName} 找到 DialogueSystemManager 类型");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("[GameManager] 直接找到了 DialogueSystemManager 类型");
            }
            
            if (dialogueSystemManagerType == null)
            {
                Debug.LogError("[GameManager] 无法找到 NewDialogueSystem.DialogueSystemManager 类型！请检查：");
                Debug.LogError("  1. 脚本是否已编译");
                Debug.LogError("  2. 命名空间是否正确：NewDialogueSystem");
                Debug.LogError("  3. 类名是否正确：DialogueSystemManager");
                
                // 列出所有已加载的程序集用于调试
                Debug.Log("[GameManager] 已加载的程序集列表：");
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Debug.Log($"  - {assembly.FullName}");
                }
                return;
            }

            Debug.Log($"[GameManager] DialogueSystemManager 类型信息：{dialogueSystemManagerType.FullName}");

            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogError("[GameManager] DialogueSystemManager 没有 Instance 属性！");
                return;
            }

            var instance = instanceProperty.GetValue(null);
            Debug.Log($"[GameManager] 当前 Instance 值: {(instance == null ? "null" : "已存在")}");
            
            if (instance == null)
            {
                Debug.Log("[GameManager] Instance 为 null，开始创建 DialogueSystemManager GameObject...");
                
                // 再次检查场景中是否已经存在（可能是在检查期间被创建了）
                GameObject existingInScene = GameObject.Find("DialogueSystemManager");
                if (existingInScene != null)
                {
                    Debug.Log("[GameManager] 场景中已存在 DialogueSystemManager GameObject，尝试获取组件...");
                    var existingComponent = existingInScene.GetComponent(dialogueSystemManagerType);
                    if (existingComponent != null)
                    {
                        Debug.Log("[GameManager] 找到了现有的 DialogueSystemManager 组件");
                        // 等待一帧后验证
                        StartCoroutine(VerifyDialogueSystemManagerCreated());
                        return;
                    }
                }
                
                GameObject dialogueManagerObj = new GameObject("DialogueSystemManager");
                Debug.Log($"[GameManager] 已创建空 GameObject: {dialogueManagerObj.name}，场景: {dialogueManagerObj.scene.name}");
                
                // 不设为子对象，让它作为根对象（DialogueSystemManager的Awake会处理DontDestroyOnLoad）
                dialogueManagerObj.transform.SetParent(null);
                Debug.Log($"[GameManager] 已将 GameObject 设为根对象，场景: {dialogueManagerObj.scene.name}");
                
                // 添加组件
                Component component = null;
                try
                {
                    component = dialogueManagerObj.AddComponent(dialogueSystemManagerType);
                    Debug.Log($"[GameManager] 已添加 DialogueSystemManager 组件: {(component != null ? "成功" : "失败")}");
                    if (component != null)
                    {
                        Debug.Log($"[GameManager] 组件类型: {component.GetType().FullName}");
                    }
                }
                catch (System.Exception addCompEx)
                {
                    Debug.LogError($"[GameManager] 添加组件时发生错误: {addCompEx.Message}\n{addCompEx.StackTrace}");
                    if (dialogueManagerObj != null)
                    {
                        Destroy(dialogueManagerObj);
                    }
                    return;
                }
                
                if (component == null)
                {
                    Debug.LogError("[GameManager] 组件添加失败，component 为 null！");
                    if (dialogueManagerObj != null)
                    {
                        Destroy(dialogueManagerObj);
                    }
                    return;
                }
                
                Debug.Log($"[GameManager] DialogueSystemManager GameObject 创建完成，名称: {dialogueManagerObj.name}，场景: {dialogueManagerObj.scene.name}，激活状态: {dialogueManagerObj.activeSelf}");
                Debug.Log("[GameManager] 等待 Awake 执行...");
                
                // 立即检查一次Instance（Awake可能在AddComponent时已经执行）
                var instanceAfterCreate = instanceProperty.GetValue(null);
                Debug.Log($"[GameManager] 创建后立即检查 Instance: {(instanceAfterCreate == null ? "null" : "已存在")}");
                
                // 等待一帧后检查Instance是否已设置
                StartCoroutine(VerifyDialogueSystemManagerCreated());
            }
            else
            {
                Debug.Log("[GameManager] DialogueSystemManager Instance 已存在，无需创建");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 初始化对话系统时发生错误: {e.Message}\n{e.StackTrace}");
        }

        // 订阅对话结束事件，用于从剧情模式切换回桌面模式（延迟到下一帧，确保Instance已设置）
        StartCoroutine(SubscribeToDialogueEventsDelayed());
    }

    /// <summary>
    /// 验证DialogueSystemManager是否已正确创建
    /// </summary>
    private System.Collections.IEnumerator VerifyDialogueSystemManagerCreated()
    {
        yield return null; // 等待一帧

        Debug.Log("[GameManager] 开始验证 DialogueSystemManager 是否已创建...");
        
        // 检查GameObject是否存在
        GameObject obj = GameObject.Find("DialogueSystemManager");
        if (obj == null)
        {
            Debug.LogError("[GameManager] DialogueSystemManager GameObject 不存在！可能已被销毁。");
            yield break;
        }
        
        Debug.Log($"[GameManager] DialogueSystemManager GameObject 存在: {obj.name}, 激活状态: {obj.activeSelf}, 层级: {obj.activeInHierarchy}");

        // 查找类型
        System.Type dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
        if (dialogueSystemManagerType == null)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                dialogueSystemManagerType = assembly.GetType("NewDialogueSystem.DialogueSystemManager");
                if (dialogueSystemManagerType != null) break;
            }
        }

        if (dialogueSystemManagerType == null)
        {
            Debug.LogError("[GameManager] 无法找到 DialogueSystemManager 类型！");
            yield break;
        }

        // 检查组件是否存在
        var component = obj.GetComponent(dialogueSystemManagerType);
        if (component == null)
        {
            Debug.LogError("[GameManager] DialogueSystemManager 组件不存在于 GameObject 上！");
            yield break;
        }
        
        Debug.Log($"[GameManager] DialogueSystemManager 组件存在: {component.GetType().Name}");
        
        // 验证Instance
        bool instanceFound = false;
        try
        {
            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    Debug.Log("[GameManager] ✓ DialogueSystemManager.Instance 已成功设置！");
                    Debug.Log($"[GameManager] Instance GameObject: {((MonoBehaviour)instance).gameObject.name}");
                    instanceFound = true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 验证Instance属性时发生错误: {e.Message}\n{e.StackTrace}");
        }

        // 如果第一次检查Instance为null，再等待一帧
        if (!instanceFound)
        {
            Debug.LogWarning("[GameManager] DialogueSystemManager.Instance 仍为 null，可能 Awake 尚未执行，再等待一帧...");
            yield return null; // 再等待一帧
            
            try
            {
                var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        Debug.Log("[GameManager] ✓ DialogueSystemManager.Instance 已成功设置（延迟）！");
                    }
                    else
                    {
                        Debug.LogError("[GameManager] DialogueSystemManager.Instance 仍为 null！Awake 可能没有正确执行。");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] 第二次验证Instance属性时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    /// <summary>
    /// 延迟订阅对话系统事件（确保DialogueSystemManager已完全初始化）
    /// </summary>
    private System.Collections.IEnumerator SubscribeToDialogueEventsDelayed()
    {
        yield return null; // 等待一帧，确保DialogueSystemManager的Awake已完成
        
        SubscribeToDialogueEvents();
    }

    /// <summary>
    /// 延迟配置对话系统（确保DialogueSystemManager已创建）
    /// </summary>
    private System.Collections.IEnumerator ConfigureDialogueSystemDelayed()
    {
        // 等待几帧，确保DialogueSystemManager已创建和初始化
        yield return null;
        yield return null;
        
        // 如果DialogueSystemManager还不存在，再次尝试创建
        GameObject existingObj = GameObject.Find("DialogueSystemManager");
        if (existingObj == null)
        {
            Debug.LogWarning("[GameManager] DialogueSystemManager 仍不存在，在 Start 中再次尝试创建...");
            InitializeDialogueSystem();
            
            // 再等待一帧
            yield return null;
        }
        
        // 配置对话系统UI预制体
        ConfigureDialogueSystemUI();
    }

    /// <summary>
    /// 配置对话系统UI预制体和数据库（新对话系统，使用反射避免编译错误）
    /// </summary>
    private void ConfigureDialogueSystemUI()
    {
        if (currentPetProfile == null)
            return;

        try
        {
            var dialogueSystemManagerType = System.Type.GetType("NewDialogueSystem.DialogueSystemManager");
            if (dialogueSystemManagerType == null)
            {
                Debug.LogError("[GameManager] 无法找到 NewDialogueSystem.DialogueSystemManager 类型");
                return;
            }

            var instanceProperty = dialogueSystemManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogError("[GameManager] DialogueSystemManager 没有 Instance 属性");
                return;
            }

            var instance = instanceProperty.GetValue(null);
            if (instance == null)
            {
                Debug.LogError("[GameManager] DialogueSystemManager.Instance 为 null，无法配置对话系统");
                return;
            }

            // 配置UI预制体（从PetProfileSO读取）
            if (currentPetProfile.storyDialogueUIPrefab != null)
            {
                var storyPrefabField = dialogueSystemManagerType.GetField("defaultStoryDialogueUIPrefab");
                if (storyPrefabField != null)
                {
                    storyPrefabField.SetValue(instance, currentPetProfile.storyDialogueUIPrefab);
                }
            }
            if (currentPetProfile.bubbleDialogueUIPrefab != null)
            {
                var bubblePrefabField = dialogueSystemManagerType.GetField("defaultBubbleDialogueUIPrefab");
                if (bubblePrefabField != null)
                {
                    bubblePrefabField.SetValue(instance, currentPetProfile.bubbleDialogueUIPrefab);
                }
            }
            if (currentPetProfile.choiceDialogueUIPrefab != null)
            {
                var choicePrefabField = dialogueSystemManagerType.GetField("defaultChoiceDialogueUIPrefab");
                if (choicePrefabField != null)
                {
                    choicePrefabField.SetValue(instance, currentPetProfile.choiceDialogueUIPrefab);
                }
            }
            if (currentPetProfile.historyUIPrefab != null)
            {
                var historyPrefabField = dialogueSystemManagerType.GetField("defaultHistoryDialogueUIPrefab");
                if (historyPrefabField != null)
                {
                    historyPrefabField.SetValue(instance, currentPetProfile.historyUIPrefab);
                }
            }

            // 重新初始化默认UI预制体字典（配置完预制体后需要更新字典）
            var initializeUIPrefabsMethod = dialogueSystemManagerType.GetMethod("InitializeDefaultUIPrefabs");
            if (initializeUIPrefabsMethod != null)
            {
                initializeUIPrefabsMethod.Invoke(instance, null);
                Debug.Log("[GameManager] 已重新初始化对话系统UI预制体字典");
            }

            // 加载对话数据库
            if (!string.IsNullOrEmpty(currentPetProfile.dialogueDatabasePath))
            {
                var databasePathField = dialogueSystemManagerType.GetField("databaseJsonPath");
                if (databasePathField != null)
                {
                    databasePathField.SetValue(instance, currentPetProfile.dialogueDatabasePath);
                }

                var loadMethod = dialogueSystemManagerType.GetMethod("LoadDialogueDatabase");
                if (loadMethod != null)
                {
                    loadMethod.Invoke(instance, null);
                }
            }

            Debug.Log("[GameManager] 对话系统UI和数据库配置完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 配置对话系统时发生错误: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 当对话会话结束时，此方法会被调用（新对话系统）
    /// </summary>
    private void OnDialogueSessionEnded(string sessionID)
    {
        Debug.Log("对话会话结束，返回桌面模式...");
        var pet = PetManager.Instance != null ? PetManager.Instance.ActivePet : null;
        if (pet == null) return;

        // 重新启用桌面模式AI和交互
        if (pet.StateMachine != null) pet.StateMachine.enabled = true;
        if (pet.GetComponent<PetInteraction>() != null) pet.GetComponent<PetInteraction>().enabled = true;

        // 关键：返回桌面模式时，立即重新计算并更新宠物的桌面活动范围
        pet.UpdateWalkableArea();
    }

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
            Debug.LogWarning($"[GameManager] 订阅对话系统事件时发生错误: {e.Message}");
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
            Debug.LogWarning($"[GameManager] 取消订阅对话系统事件时发生错误: {e.Message}");
        }
    }
}