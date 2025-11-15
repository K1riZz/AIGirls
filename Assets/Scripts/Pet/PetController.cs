using UnityEngine;
using PixelCrushers.DialogueSystem;
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
    private Coroutine barkThenHideCoroutine;

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
    }

    void Start()
    {
        // 如果Profile尚未初始化（意味着它不是由PetManager动态生成的），
        // 则使用在Inspector中指定的initialProfile进行初始化。
        if (Profile == null && initialProfile != null)
        {
            Initialize(initialProfile);
        }

        // 订阅对话系统事件，以便在对话开始时强制进入idle状态
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.conversationStarted += OnConversationStarted;
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.conversationStarted -= OnConversationStarted;
        }
    }

    public void Initialize(PetProfileSO profile)
    {
        this.Profile = profile;
        this.name = $"Pet_{profile.petName}";

        // 初始化好感度等数值, 从Dialogue System的变量中读取
        Affection = DialogueLua.GetVariable("Affection").AsFloat;

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
            Debug.LogError("引用失败.", this);
            return;
        }

        // 如果右键菜单是打开的，则左键点击任何地方（包括宠物自己）都应关闭菜单
        if (bottomMenuGroup != null && bottomMenuGroup.alpha > 0)
        {
            HideContextMenus();
            return;
        }

        // 触发一个简单的对话
        if (!string.IsNullOrEmpty(Profile.touchConversationTitle))
        {
            // 如果没有其他对话正在进行，则触发点击对话
            if (!DialogueManager.IsConversationActive)
            {
                TriggerBark(Profile.touchConversationTitle, Profile.touchConversationDuration);
            }
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
        // 根据Profile中是否存在剧情对话来决定是否显示"剧情"按钮
        if (storyModeButton != null)
        {
            bool isStoryAvailable = (Profile != null && !string.IsNullOrEmpty(Profile.startConversationTitle));
            storyModeButton.SetActive(isStoryAvailable);
        }

        // 显示菜单时，强制进入idle状态
        ForceIdleState();

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
    /// 触发一个会自动隐藏的Bark对话。
    /// </summary>
    /// <param name="conversationTitle">对话标题</param>
    /// <param name="duration">显示时长（秒）</param>
    public void TriggerBark(string conversationTitle, float duration)
    {
        if (string.IsNullOrEmpty(conversationTitle) || duration <= 0 || DialogueManager.IsConversationActive)
        {
            return;
        }
        
       // 停止之前可能正在运行的任何BarkThenHide协程，防止UI冲突
        if (barkThenHideCoroutine != null) StopCoroutine(barkThenHideCoroutine);
        barkThenHideCoroutine = StartCoroutine(BarkThenHide(conversationTitle, duration));

    }
    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }

    #region 右键菜单按钮功能

    public void EnterStoryMode()
    {
        HideContextMenus();
        Debug.Log("进入剧情模式...");
        // 强制进入idle状态
        ForceIdleState();
        // 实际的剧情触发逻辑
        if (!string.IsNullOrEmpty(Profile.startConversationTitle))
        {
            DialogueManager.StartConversation(Profile.startConversationTitle, this.transform);
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
        // 检查对话是否激活
        if (DialogueManager.IsConversationActive)
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
    /// 对话开始时的事件处理
    /// </summary>
    private void OnConversationStarted(Transform actor)
    {
        Debug.Log("[PetController] 对话开始，强制进入idle状态");
        ForceIdleState();
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

        // 使用Dialogue System的BarkString来显示任意文本
        // 这会打断任何当前正在显示的Bark
        // BarkString(文本, 说话者, 听者, 序列)
        DialogueManager.BarkString(message, this.transform);

        Debug.Log($"[PetController] 显示玩家输入: '{message}'");
        // 重置闲聊计时器，避免立即触发闲聊
        ResetIdleChatterTimer();
    }

    /// <summary>
    /// 安全地隐藏当前正在显示的任何Bark气泡，避免调用StopAllCoroutines()。
    /// </summary>
    public void SafelyHideCurrentBark()
    {
        var barkUI = DialogueActor.GetBarkUI(this.transform);
        if (barkUI != null && barkUI.isPlaying)
        {
            var standardBarkUI = barkUI as StandardBarkUI;
            if (standardBarkUI != null)
            {
                StartCoroutine(FadeOutAndDisableBark(standardBarkUI));
            }
            else
            {
                // 对于非标准UI，只能调用原始的Hide方法
                barkUI.Hide();
            }
        }
    }

    private IEnumerator FadeOutAndDisableBark(StandardBarkUI barkUI)
    {
        var barkCanvasGroup = barkUI.GetComponent<CanvasGroup>();
        if (barkCanvasGroup != null && barkCanvasGroup.alpha > 0)
        {
            float fadeTime = 0.2f;
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                barkCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeTime);
                yield return null;
            }
        }
        barkUI.gameObject.SetActive(false); // 直接禁用，不调用Hide()
    }

private IEnumerator BarkThenHide(string conversationTitle, float duration)
    {
        // 1. 触发Bark，让它显示出来
        // DialogueManager.Bark 会找到或创建一个 Bark UI 并让它显示内容。
        // 我们在它显示后立即获取这个UI的引用。
        DialogueManager.Bark(conversationTitle, this.transform);
        Debug.Log($"[PetController] Bark '{conversationTitle}' 显示，持续 {duration} 秒。");
        
        // 获取刚刚被激活的Bark UI实例
        var barkUI = DialogueActor.GetBarkUI(this.transform);
        var standardBarkUI = barkUI as StandardBarkUI;
        
        // 2. 等待指定的时长。
        yield return new WaitForSeconds(duration);
        
        // 3. 安全地隐藏这个协程自己创建的UI实例
        // 检查UI实例是否存在，并且它的alpha值大于0（意味着它当前是可见的）
        if (standardBarkUI != null && standardBarkUI.gameObject.activeInHierarchy )
        {
            var barkCanvasGroup = standardBarkUI.GetComponent<CanvasGroup>();
            if (barkCanvasGroup != null && barkCanvasGroup.alpha > 0)
            {
                Debug.Log($"[PetController] Bark '{conversationTitle}' 协程结束，正在手动隐藏其UI。");
                // 手动实现淡出动画
                float fadeTime = 0.2f;
                float timer = 0f;
                while (timer < fadeTime)
                {
                    timer += Time.deltaTime;
                    barkCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeTime);
                    yield return null;
                }
                
                // 动画结束后，禁用交互并直接禁用GameObject，完全绕过Hide()方法
                barkCanvasGroup.interactable = false;
                barkCanvasGroup.blocksRaycasts = false;
                standardBarkUI.gameObject.SetActive(false);
            }
        }
        barkThenHideCoroutine = null;
    }


}