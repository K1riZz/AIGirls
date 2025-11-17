using UnityEngine;
using System.Collections;

/// <summary>
/// 快速点击检测器，用于检测玩家在短时间内快速连续点击桌宠
/// </summary>
public class RapidClickDetector : MonoBehaviour
{
    [Header("点击检测配置")]
    [Tooltip("是否使用PetProfileSO的配置（推荐）")]
    [SerializeField] private bool useProfileConfig = true;
    
    [Header("本地配置（仅当useProfileConfig=false时使用）")]
    [Tooltip("触发小游戏所需的点击次数")]
    [SerializeField] private int requiredClicks = 10;
    [Tooltip("检测快速点击的时间窗口（秒）")]
    [SerializeField] private float clickTimeWindow = 2f;
    [Tooltip("两次点击之间的最大时间间隔（秒），超过此时间则重置计数")]
    [SerializeField] private float maxClickInterval = 0.5f;

    // 实际使用的配置值
    private int actualRequiredClicks;
    private float actualClickTimeWindow;
    private float actualMaxClickInterval;

    private int currentClickCount = 0;
    private float lastClickTime = 0f;
    private float windowStartTime = 0f;
    
    private MiniGameManager miniGameManager;
    private PetController petController;

    void Awake()
    {
        Debug.Log("[RapidClickDetector] Awake - 开始初始化");
        
        petController = GetComponentInParent<PetController>();
        if (petController == null)
        {
            Debug.LogError("[RapidClickDetector] 未找到PetController!", this);
            enabled = false;
            return;
        }
        Debug.Log($"[RapidClickDetector] 找到PetController: {petController.name}");

        // 加载配置
        LoadConfig();

        // 查找MiniGameManager（延迟查找，因为可能在Start时才创建）
        StartCoroutine(FindMiniGameManagerDelayed());
    }

    /// <summary>
    /// 延迟查找MiniGameManager
    /// </summary>
    private System.Collections.IEnumerator FindMiniGameManagerDelayed()
    {
        yield return null; // 等待一帧，确保MiniGameManager已创建

        if (miniGameManager == null)
        {
            miniGameManager = FindObjectOfType<MiniGameManager>();
            if (miniGameManager == null)
            {
                Debug.LogWarning("[RapidClickDetector] MiniGameManager未找到，将在RegisterClick时再次查找");
            }
            else
            {
                Debug.Log($"[RapidClickDetector] 找到MiniGameManager: {miniGameManager.name}");
            }
        }
    }

    /// <summary>
    /// 记录一次点击
    /// </summary>
    public void RegisterClick()
    {
        Debug.Log($"[RapidClickDetector] 收到点击事件");

        // 检查是否在剧情模式中，如果是则不触发小游戏
        if (PixelCrushers.DialogueSystem.DialogueManager.IsConversationActive)
        {
            Debug.Log("[RapidClickDetector] 当前在剧情模式中，忽略点击");
            return;
        }

        // 检查是否已经在小游戏中
        if (miniGameManager != null && miniGameManager.IsMiniGameActive)
        {
            Debug.Log("[RapidClickDetector] 当前正在小游戏中，忽略点击");
            return;
        }

        // 确保MiniGameManager存在
        if (miniGameManager == null)
        {
            miniGameManager = FindObjectOfType<MiniGameManager>();
            if (miniGameManager == null)
            {
                Debug.LogWarning("[RapidClickDetector] MiniGameManager未找到！");
                return;
            }
        }

        float currentTime = Time.time;

        // 如果距离上次点击超过最大间隔，重置计数
        if (currentTime - lastClickTime > actualMaxClickInterval && currentClickCount > 0)
        {
            Debug.Log($"[RapidClickDetector] 点击间隔过长({currentTime - lastClickTime:F2}秒 > {actualMaxClickInterval}秒)，重置计数");
            ResetClickCount();
        }

        // 如果是第一次点击，记录窗口开始时间
        if (currentClickCount == 0)
        {
            windowStartTime = currentTime;
            Debug.Log($"[RapidClickDetector] 开始新的点击窗口");
        }

        currentClickCount++;
        lastClickTime = currentTime;

        float elapsedTime = currentTime - windowStartTime;
        Debug.Log($"[RapidClickDetector] 点击计数: {currentClickCount}/{actualRequiredClicks}, 已用时: {elapsedTime:F2}/{actualClickTimeWindow}秒");

        // 检查是否达到所需点击次数且在时间窗口内
        if (currentClickCount >= actualRequiredClicks)
        {
            if (elapsedTime <= actualClickTimeWindow)
            {
                // 触发小游戏
                Debug.Log($"[RapidClickDetector] 达到触发条件！点击{currentClickCount}次，用时{elapsedTime:F2}秒");
                OnRapidClickDetected();
                ResetClickCount();
            }
            else
            {
                Debug.Log($"[RapidClickDetector] 点击次数达标但超出时间窗口({elapsedTime:F2}秒 > {actualClickTimeWindow}秒)，重置计数");
                ResetClickCount();
            }
        }
    }

    /// <summary>
    /// 从PetProfileSO或本地配置加载参数
    /// </summary>
    private void LoadConfig()
    {
        if (useProfileConfig && petController != null && petController.Profile != null)
        {
            var profile = petController.Profile;
            actualRequiredClicks = profile.rapidClickRequiredClicks;
            actualClickTimeWindow = profile.rapidClickTimeWindow;
            actualMaxClickInterval = profile.rapidClickMaxInterval;
            
            Debug.Log($"[RapidClickDetector] 从PetProfileSO加载配置 - 所需点击: {actualRequiredClicks}, 时间窗口: {actualClickTimeWindow}秒");
        }
        else
        {
            actualRequiredClicks = requiredClicks;
            actualClickTimeWindow = clickTimeWindow;
            actualMaxClickInterval = maxClickInterval;
            
            Debug.Log($"[RapidClickDetector] 使用本地配置 - 所需点击: {actualRequiredClicks}, 时间窗口: {actualClickTimeWindow}秒");
        }
    }

    /// <summary>
    /// 重置点击计数
    /// </summary>
    private void ResetClickCount()
    {
        currentClickCount = 0;
        windowStartTime = 0f;
        lastClickTime = 0f;
    }

    /// <summary>
    /// 当检测到快速点击时调用
    /// </summary>
    private void OnRapidClickDetected()
    {
        Debug.Log($"[RapidClickDetector] 检测到快速点击！触发小游戏");
        
        // 确保MiniGameManager存在
        if (miniGameManager == null)
        {
            miniGameManager = FindObjectOfType<MiniGameManager>();
            if (miniGameManager == null)
            {
                Debug.LogError("[RapidClickDetector] 无法找到MiniGameManager！");
                return;
            }
        }

        if (miniGameManager != null && petController != null)
        {
            Debug.Log($"[RapidClickDetector] 调用MiniGameManager.StartRandomMiniGame");
            miniGameManager.StartRandomMiniGame(petController);
        }
        else
        {
            Debug.LogError($"[RapidClickDetector] MiniGameManager或PetController为null！MiniGameManager: {miniGameManager}, PetController: {petController}");
        }
    }

    /// <summary>
    /// 在Inspector中设置配置参数（本地配置，仅当useProfileConfig=false时有效）
    /// </summary>
    public void SetConfig(int clicks, float window, float interval)
    {
        requiredClicks = clicks;
        clickTimeWindow = window;
        maxClickInterval = interval;
        
        // 如果使用本地配置，立即应用
        if (!useProfileConfig)
        {
            LoadConfig();
        }
    }
}

