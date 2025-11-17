using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 小游戏管理器，负责管理所有小游戏
/// </summary>
public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [Header("小游戏配置")]
    [Tooltip("可用的小游戏列表")]
    private List<MiniGameBase> availableMiniGames = new List<MiniGameBase>();

    [Header("UI容器")]
    [Tooltip("小游戏UI容器，应该位于MainCanvas下")]
    private Transform miniGameUIContainer;

    private MiniGameBase currentMiniGame;
    public bool IsMiniGameActive => currentMiniGame != null && currentMiniGame.IsActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 如果没有指定UI容器，尝试自动查找
        // 注意：根据UI层级结构，MiniGameContainer应该在Pet层级下
        if (miniGameUIContainer == null)
        {
            // 首先尝试在Pet对象下查找
            PetController activePet = PetManager.Instance != null ? PetManager.Instance.ActivePet : null;
            if (activePet != null)
            {
                Transform petTransform = activePet.transform;
                Transform container = petTransform.Find("MiniGameContainer");
                if (container == null)
                {
                    // 在Pet下创建容器
                    GameObject containerObj = new GameObject("MiniGameContainer");
                    containerObj.transform.SetParent(petTransform, false);
                    RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                    
                    CanvasGroup canvasGroup = containerObj.AddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    
                    miniGameUIContainer = containerObj.transform;
                    Debug.Log("[MiniGameManager] 在Pet层级下创建了MiniGameContainer");
                }
                else
                {
                    miniGameUIContainer = container;
                    Debug.Log("[MiniGameManager] 找到了现有的MiniGameContainer");
                }
            }
            else
            {
                // 如果Pet还没有创建，等待Pet创建后再初始化
                Debug.LogWarning("[MiniGameManager] Pet还未创建，将在Pet创建后初始化UI容器");
                StartCoroutine(WaitForPetAndInitialize());
                return;
            }
        }

        // 初始化所有小游戏
        InitializeMiniGames();
    }

    /// <summary>
    /// 等待Pet创建后初始化UI容器
    /// </summary>
    private System.Collections.IEnumerator WaitForPetAndInitialize()
    {
        // 等待Pet创建
        while (PetManager.Instance == null || PetManager.Instance.ActivePet == null)
        {
            yield return null;
        }

        // Pet创建后，查找或创建容器
        PetController activePet = PetManager.Instance.ActivePet;
        Transform petTransform = activePet.transform;
        Transform container = petTransform.Find("MiniGameContainer");
        
        if (container == null)
        {
            GameObject containerObj = new GameObject("MiniGameContainer");
            containerObj.transform.SetParent(petTransform, false);
            RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            CanvasGroup canvasGroup = containerObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            miniGameUIContainer = containerObj.transform;
            Debug.Log("[MiniGameManager] 在Pet层级下创建了MiniGameContainer");
        }
        else
        {
            miniGameUIContainer = container;
            Debug.Log("[MiniGameManager] 找到了现有的MiniGameContainer");
        }

        // 初始化所有小游戏
        InitializeMiniGames();
    }

    /// <summary>
    /// 初始化所有小游戏
    /// </summary>
    private void InitializeMiniGames()
    {
        foreach (var miniGame in availableMiniGames)
        {
            if (miniGame != null)
            {
                miniGame.Initialize(miniGameUIContainer);
            }
        }
    }

    /// <summary>
    /// 启动随机小游戏
    /// </summary>
    public void StartRandomMiniGame(PetController petController)
    {
        Debug.Log("[MiniGameManager] StartRandomMiniGame 被调用");

        if (petController == null)
        {
            Debug.LogError("[MiniGameManager] PetController为null！");
            return;
        }

        // 如果UI容器还未创建，尝试创建
        if (miniGameUIContainer == null)
        {
            PetController activePet = PetManager.Instance != null ? PetManager.Instance.ActivePet : petController;
            if (activePet != null)
            {
                Transform petTransform = activePet.transform;
                Transform container = petTransform.Find("MiniGameContainer");
                if (container == null)
                {
                    GameObject containerObj = new GameObject("MiniGameContainer");
                    containerObj.transform.SetParent(petTransform, false);
                    RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                    
                    CanvasGroup canvasGroup = containerObj.AddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    
                    miniGameUIContainer = containerObj.transform;
                    Debug.Log("[MiniGameManager] 在StartRandomMiniGame中创建了MiniGameContainer");
                }
                else
                {
                    miniGameUIContainer = container;
                }
            }
        }

        // 如果已有小游戏在运行，则先结束它
        if (IsMiniGameActive)
        {
            Debug.Log("[MiniGameManager] 当前有小游戏运行中，先结束它");
            EndCurrentMiniGame();
        }

        // 从可用的小游戏中随机选择一个
        if (availableMiniGames.Count == 0)
        {
            Debug.LogWarning("[MiniGameManager] 没有可用的小游戏！");
            return;
        }

        // 过滤出可用的小游戏（不为null且已初始化）
        List<MiniGameBase> validMiniGames = new List<MiniGameBase>();
        foreach (var miniGame in availableMiniGames)
        {
            if (miniGame != null)
            {
                if (!miniGame.IsInitialized && miniGameUIContainer != null)
                {
                    miniGame.Initialize(miniGameUIContainer);
                }
                if (miniGame.IsInitialized)
                {
                    validMiniGames.Add(miniGame);
                }
            }
        }

        if (validMiniGames.Count == 0)
        {
            Debug.LogWarning("[MiniGameManager] 没有有效的小游戏！");
            return;
        }

        int randomIndex = Random.Range(0, validMiniGames.Count);
        currentMiniGame = validMiniGames[randomIndex];

        Debug.Log($"[MiniGameManager] 启动小游戏: {currentMiniGame.GetType().Name} (共{validMiniGames.Count}个小游戏可选)");
        currentMiniGame.StartGame(petController);
    }

    /// <summary>
    /// 启动指定的小游戏
    /// </summary>
    public void StartMiniGame(MiniGameBase miniGame, PetController petController)
    {
        if (miniGame == null || !availableMiniGames.Contains(miniGame))
        {
            Debug.LogWarning("[MiniGameManager] 无效的小游戏！");
            return;
        }

        if (IsMiniGameActive)
        {
            EndCurrentMiniGame();
        }

        currentMiniGame = miniGame;
        currentMiniGame.StartGame(petController);
    }

    /// <summary>
    /// 结束当前小游戏
    /// </summary>
    public void EndCurrentMiniGame()
    {
        if (currentMiniGame != null)
        {
            currentMiniGame.EndGame();
            currentMiniGame = null;
        }
    }

    /// <summary>
    /// 注册小游戏
    /// </summary>
    public void RegisterMiniGame(MiniGameBase miniGame)
    {
        if (miniGame != null && !availableMiniGames.Contains(miniGame))
        {
            availableMiniGames.Add(miniGame);
            if (miniGameUIContainer != null)
            {
                miniGame.Initialize(miniGameUIContainer);
            }
        }
    }

    /// <summary>
    /// 取消注册小游戏
    /// </summary>
    public void UnregisterMiniGame(MiniGameBase miniGame)
    {
        if (availableMiniGames.Contains(miniGame))
        {
            availableMiniGames.Remove(miniGame);
        }
    }
}

