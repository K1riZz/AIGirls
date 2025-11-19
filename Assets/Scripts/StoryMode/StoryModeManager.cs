using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 剧情模式管理器，负责管理剧情模式的进入和退出
/// </summary>
public class StoryModeManager : MonoBehaviour
{
    public static StoryModeManager Instance { get; private set; }

    [Header("壁纸位置")]
    [Tooltip("壁纸Canvas（自动创建，独立于MainCanvas）")]
    private Canvas wallpaperCanvas;

    private GameObject wallpaperInstance;
    private const int WALLPAPER_CANVAS_SORTING_ORDER = 50; // 比MainCanvas的100低，确保在下方
    private const int MAIN_CANVAS_SORTING_ORDER = 100; // MainCanvas prefab的SortingOrder
    private bool isInStoryMode = false;
    private Vector2 savedPetPosition; // 保存进入剧情模式前的位置
    private RectTransform savedPetRectTransform; // 保存桌宠的RectTransform引用

    // 剧情模式状态事件
    public System.Action OnStoryModeEntered;
    public System.Action OnStoryModeExited;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 创建独立的壁纸Canvas（作为场景根对象，与MainCanvas完全解耦）
        CreateWallpaperCanvas();
        
        // 延迟初始化壁纸（等待Pet实例化）
        StartCoroutine(InitializeWallpaperDelayed());
    }

    /// <summary>
    /// 创建独立的壁纸Canvas（作为场景根对象）
    /// </summary>
    private void CreateWallpaperCanvas()
    {
        // 查找或创建WallpaperCanvas
        GameObject wallpaperCanvasObj = GameObject.Find("WallpaperCanvas");
        if (wallpaperCanvasObj == null)
        {
            wallpaperCanvasObj = new GameObject("WallpaperCanvas");
            wallpaperCanvasObj.transform.SetParent(null); // 作为场景根对象
            
            // 添加Canvas组件
            wallpaperCanvas = wallpaperCanvasObj.AddComponent<Canvas>();
            wallpaperCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            wallpaperCanvas.sortingOrder = WALLPAPER_CANVAS_SORTING_ORDER; // 比MainCanvas低，确保在下方
            
            // 添加CanvasScaler（可选，用于适配不同分辨率）
            UnityEngine.UI.CanvasScaler scaler = wallpaperCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster（用于UI交互）
            wallpaperCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            Debug.Log($"[StoryModeManager] 创建独立的WallpaperCanvas，SortingOrder: {WALLPAPER_CANVAS_SORTING_ORDER}");
        }
        else
        {
            wallpaperCanvas = wallpaperCanvasObj.GetComponent<Canvas>();
            if (wallpaperCanvas == null)
            {
                wallpaperCanvas = wallpaperCanvasObj.AddComponent<Canvas>();
                wallpaperCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                wallpaperCanvas.sortingOrder = WALLPAPER_CANVAS_SORTING_ORDER;
            }
            Debug.Log("[StoryModeManager] 找到已存在的WallpaperCanvas");
        }
    }

    /// <summary>
    /// 延迟初始化壁纸（等待Pet实例化）
    /// </summary>
    private System.Collections.IEnumerator InitializeWallpaperDelayed()
    {
        // 等待几帧，确保MainCanvas prefab已实例化
        yield return null;
        yield return null;
        yield return null;

        // 初始化桌面壁纸（游戏开始时就应该显示壁纸）
        InitializeDesktopWallpaper();
    }

    /// <summary>
    /// 初始化桌面壁纸（在游戏开始时创建壁纸）
    /// </summary>
    private void InitializeDesktopWallpaper()
    {
        // 等待Pet初始化完成
        StartCoroutine(InitializeWallpaperAfterPetReady());
    }

    /// <summary>
    /// 等待Pet准备好后初始化壁纸
    /// </summary>
    private System.Collections.IEnumerator InitializeWallpaperAfterPetReady()
    {
        // 等待PetManager初始化Pet
        while (PetManager.Instance == null || PetManager.Instance.ActivePet == null)
        {
            yield return null;
        }

        // 再等待几帧确保Pet完全初始化和层级确定
        yield return null;
        yield return null;

        PetController pet = PetManager.Instance.ActivePet;
        if (pet != null && pet.Profile != null && pet.Profile.desktopWallpaper != null)
        {
            // 创建桌面壁纸（桌面模式下也显示，作为独立的模块）
            CreateWallpaper(pet.Profile, false);
            // 确保壁纸层级和大小正确（独立于Pet）
            EnsureWallpaperLayerOrder();
            EnsureWallpaperFullscreen();
            Debug.Log("[StoryModeManager] 桌面壁纸已初始化（独立模块）");
        }
        else
        {
            Debug.LogWarning("[StoryModeManager] 无法初始化壁纸：Pet或Profile未准备好");
        }
    }

    /// <summary>
    /// 查找MainCanvas（运行时实例化的桌宠prefab）
    /// </summary>
    private Transform FindMainCanvas()
    {
        // 方法1：通过名称查找MainCanvas（桌宠prefab）
        GameObject mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas != null)
        {
            Debug.Log("[StoryModeManager] 通过名称找到MainCanvas（桌宠prefab）");
            return mainCanvas.transform;
        }

        // 方法2：查找所有Canvas，选择SortingOrder为100的（MainCanvas prefab的SortingOrder）
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases != null && canvases.Length > 0)
        {
            Canvas mainCanvasComponent = null;
            
            // 优先查找SortingOrder为100的Canvas（MainCanvas prefab）
            foreach (Canvas canvas in canvases)
            {
                if (canvas.sortingOrder == MAIN_CANVAS_SORTING_ORDER && canvas.name != "WallpaperCanvas")
                {
                    mainCanvasComponent = canvas;
                    break;
                }
            }
            
            // 如果没找到，查找SortingOrder最高的（排除WallpaperCanvas）
            if (mainCanvasComponent == null)
            {
                int highestSortOrder = int.MinValue;
                foreach (Canvas canvas in canvases)
                {
                    if (canvas.name != "WallpaperCanvas" && canvas.sortingOrder > highestSortOrder)
                    {
                        highestSortOrder = canvas.sortingOrder;
                        mainCanvasComponent = canvas;
                    }
                }
            }
            
            if (mainCanvasComponent != null)
            {
                Debug.Log($"[StoryModeManager] 通过SortingOrder找到MainCanvas: {mainCanvasComponent.name}, SortingOrder: {mainCanvasComponent.sortingOrder}");
                return mainCanvasComponent.transform;
            }
        }

        Debug.LogWarning("[StoryModeManager] 未找到MainCanvas（桌宠prefab可能尚未实例化）");
        return null;
    }

    void Update()
    {
        // 在剧情模式下检测ESC键
        if (isInStoryMode && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowExitMenu();
        }
    }

    /// <summary>
    /// 进入剧情模式
    /// </summary>
    /// <param name="conversationTitle">对话标题</param>
    public void EnterStoryMode(string conversationTitle)
    {
        if (isInStoryMode)
        {
            Debug.LogWarning("[StoryModeManager] 已经处于剧情模式中");
            return;
        }

        PetController pet = PetManager.Instance != null ? PetManager.Instance.ActivePet : null;
        if (pet == null)
        {
            Debug.LogError("[StoryModeManager] 无法进入剧情模式：没有激活的桌宠");
            return;
        }

        Debug.Log("[StoryModeManager] 进入剧情模式...");
        isInStoryMode = true;

        // ====== 剧情模式模块：完全独立于桌宠模式 ======

        // 1. 保存桌宠当前位置（用于退出剧情模式时恢复）
        SavePetPosition(pet);

        // 2. 确保wallpaperCanvas已创建
        if (wallpaperCanvas == null)
        {
            CreateWallpaperCanvas();
            if (wallpaperCanvas == null)
            {
                Debug.LogError("[StoryModeManager] 无法进入剧情模式：无法创建WallpaperCanvas");
                isInStoryMode = false;
                return;
            }
        }

        // 3. 剧情模式：确保壁纸存在并全屏显示（独立模块）
        if (wallpaperInstance == null || wallpaperInstance.GetComponent<Image>().sprite != pet.Profile.desktopWallpaper)
        {
            CreateWallpaper(pet.Profile, true);
        }
        // 确保壁纸全屏且在最底层
        EnsureWallpaperFullscreen();
        EnsureWallpaperLayerOrder();

        // 4. 桌宠模式：隐藏桌宠（剧情模式下不需要桌宠）
        HidePet(pet);

        // 5. 桌宠模式：禁用桌宠的AI和交互（剧情模式不需要）
        if (pet.StateMachine != null)
        {
            pet.StateMachine.enabled = false;
        }
        if (pet.GetComponent<PetInteraction>() != null)
        {
            pet.GetComponent<PetInteraction>().enabled = false;
        }

        // 6. 系统级：隐藏桌面图标（切换到全屏AVG模式）
        if (DesktopIconController.Instance != null)
        {
            DesktopIconController.Instance.HideDesktopIcons();
        }

        // 7. 系统级：切换到全屏模式
        if (WindowsController.Instance != null)
        {
            WindowsController.Instance.EnterFullscreenMode();
        }

        // 8. 剧情模式：触发对话
        if (!string.IsNullOrEmpty(conversationTitle))
        {
            DialogueManager.StartConversation(conversationTitle, pet.transform);
        }

        // 9. 触发事件
        OnStoryModeEntered?.Invoke();
        
        Debug.Log("[StoryModeManager] 剧情模式已激活（壁纸和桌宠已解耦）");
    }

    /// <summary>
    /// 退出剧情模式
    /// </summary>
    public void ExitStoryMode()
    {
        if (!isInStoryMode)
        {
            Debug.LogWarning("[StoryModeManager] 当前不在剧情模式中");
            return;
        }

        Debug.Log("[StoryModeManager] 退出剧情模式...");
        isInStoryMode = false;

        // ====== 退出剧情模式：恢复桌宠模式 ======

        PetController pet = PetManager.Instance != null ? PetManager.Instance.ActivePet : null;
        if (pet != null)
        {
            // 1. 桌宠模式：显示桌宠（退出剧情模式时桌宠应该重新出现）
            ShowPet(pet);

            // 2. 桌宠模式：重新启用桌宠的AI和交互
            if (pet.StateMachine != null)
            {
                pet.StateMachine.enabled = true;
            }
            if (pet.GetComponent<PetInteraction>() != null)
            {
                pet.GetComponent<PetInteraction>().enabled = true;
            }

            // 3. 桌宠模式：更新桌宠的活动范围
            pet.UpdateWalkableArea();
        }

        // 4. 剧情模式：不移除壁纸（壁纸作为独立模块持续存在，可同时用于桌面和剧情模式）
        // 壁纸应该在桌面模式下也显示，所以不删除
        // RemoveWallpaper(); // 不删除，壁纸是独立的背景模块

        // 5. 系统级：显示桌面图标（退出全屏AVG模式）
        if (DesktopIconController.Instance != null)
        {
            DesktopIconController.Instance.ShowDesktopIcons();
        }

        // 6. 系统级：退出全屏模式
        if (WindowsController.Instance != null)
        {
            WindowsController.Instance.ExitFullscreenMode();
        }

        // 7. 剧情模式：如果对话仍在进行，结束对话
        if (DialogueManager.IsConversationActive)
        {
            DialogueManager.StopConversation();
        }

        // 8. 触发事件
        OnStoryModeExited?.Invoke();
        
        Debug.Log("[StoryModeManager] 已退出剧情模式（桌宠模式已恢复，壁纸保持独立）");
    }

    /// <summary>
    /// 保存桌宠位置
    /// </summary>
    private void SavePetPosition(PetController pet)
    {
        if (pet != null && pet.RectTransform != null)
        {
            savedPetPosition = pet.RectTransform.anchoredPosition;
            savedPetRectTransform = pet.RectTransform;
            Debug.Log($"[StoryModeManager] 保存桌宠位置: {savedPetPosition}");
        }
    }

    /// <summary>
    /// 恢复桌宠位置
    /// </summary>
    private void RestorePetPosition(PetController pet)
    {
        if (pet != null && savedPetRectTransform != null)
        {
            savedPetRectTransform.anchoredPosition = savedPetPosition;
            Debug.Log($"[StoryModeManager] 恢复桌宠位置: {savedPetPosition}");
        }
    }

    /// <summary>
    /// 创建壁纸（完全独立于Pet模块，在独立的WallpaperCanvas下）
    /// </summary>
    /// <param name="profile">PetProfileSO配置</param>
    /// <param name="forceUpdate">是否强制更新（即使是同一个sprite）</param>
    private void CreateWallpaper(PetProfileSO profile, bool forceUpdate = false)
    {
        if (wallpaperCanvas == null)
        {
            Debug.LogWarning("[StoryModeManager] 无法创建壁纸：WallpaperCanvas未设置");
            CreateWallpaperCanvas();
            if (wallpaperCanvas == null)
            {
                Debug.LogError("[StoryModeManager] 无法创建壁纸：WallpaperCanvas创建失败");
                return;
            }
        }

        if (profile == null || profile.desktopWallpaper == null)
        {
            Debug.LogWarning("[StoryModeManager] 无法创建壁纸：PetProfileSO或壁纸Sprite未设置");
            return;
        }

        // 如果壁纸已存在且sprite相同，且不需要强制更新，则直接返回
        if (wallpaperInstance != null)
        {
            Image existingImage = wallpaperInstance.GetComponent<Image>();
            if (existingImage != null && existingImage.sprite == profile.desktopWallpaper && !forceUpdate)
            {
                Debug.Log("[StoryModeManager] 壁纸已存在，无需重新创建");
                // 确保壁纸层级和大小正确
                EnsureWallpaperLayerOrder();
                EnsureWallpaperFullscreen();
                return;
            }
            // 如果sprite不同或需要强制更新，先移除旧的
            RemoveWallpaper();
        }

        // 创建壁纸GameObject
        wallpaperInstance = new GameObject("DesktopWallpaper");
        
        // 架构修复：壁纸作为WallpaperCanvas的子对象，完全独立于MainCanvas（桌宠）
        // WallpaperCanvas和MainCanvas都是场景的根对象，彼此完全解耦
        wallpaperInstance.transform.SetParent(wallpaperCanvas.transform, false);

        // 创建RectTransform，设置为全屏
        RectTransform rectTransform = wallpaperInstance.AddComponent<RectTransform>();
        
        // 关键：设置锚点为全屏（覆盖整个屏幕）
        rectTransform.anchorMin = Vector2.zero;  // 左下角
        rectTransform.anchorMax = Vector2.one;   // 右上角
        rectTransform.sizeDelta = Vector2.zero;  // 无额外大小
        rectTransform.anchoredPosition = Vector2.zero;  // 居中
        
        // 确保位置和缩放不受父对象影响（完全独立）
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        
        // 确保壁纸全屏显示
        EnsureWallpaperFullscreen();

        // 确保壁纸在最底层（siblingIndex最小，在Pet之前）
        EnsureWallpaperLayerOrder();

        // 创建Image组件显示壁纸
        Image image = wallpaperInstance.AddComponent<Image>();
        image.sprite = profile.desktopWallpaper;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;  // 全屏显示，不保持比例
        image.raycastTarget = false;   // 壁纸不阻挡点击事件

        Debug.Log($"[StoryModeManager] 壁纸已创建（独立模块），父对象: {wallpaperCanvas.name}, " +
                  $"Canvas SortingOrder: {wallpaperCanvas.sortingOrder}, " +
                  $"层级: {wallpaperInstance.transform.GetSiblingIndex()}, " +
                  $"大小: {rectTransform.sizeDelta}, " +
                  $"锚点: ({rectTransform.anchorMin}, {rectTransform.anchorMax})");
    }

    /// <summary>
    /// 确保壁纸全屏显示（不受任何父对象限制）
    /// </summary>
    private void EnsureWallpaperFullscreen()
    {
        if (wallpaperInstance == null) return;

        RectTransform rectTransform = wallpaperInstance.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 强制设置为全屏锚点
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        
        // 确保RectTransform的pivot在中心（0.5, 0.5）
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Debug.Log($"[StoryModeManager] 壁纸全屏设置：锚点({rectTransform.anchorMin}, {rectTransform.anchorMax}), " +
                  $"大小: {rectTransform.sizeDelta}, 位置: {rectTransform.anchoredPosition}");
    }

    /// <summary>
    /// 确保壁纸层级顺序正确（WallpaperCanvas的SortingOrder低于MainCanvas）
    /// </summary>
    private void EnsureWallpaperLayerOrder()
    {
        if (wallpaperCanvas == null) return;

        // 壁纸Canvas的SortingOrder应该比MainCanvas低，确保壁纸在桌宠下方
        // MainCanvas的SortingOrder是100，WallpaperCanvas的SortingOrder是50
        if (wallpaperCanvas.sortingOrder >= MAIN_CANVAS_SORTING_ORDER)
        {
            wallpaperCanvas.sortingOrder = WALLPAPER_CANVAS_SORTING_ORDER;
            Debug.Log($"[StoryModeManager] 调整WallpaperCanvas SortingOrder为: {WALLPAPER_CANVAS_SORTING_ORDER}（确保在MainCanvas下方）");
        }
        
        // 确保壁纸在WallpaperCanvas下是第一个子对象（最底层）
        if (wallpaperInstance != null)
        {
            wallpaperInstance.transform.SetSiblingIndex(0);
        }
    }

    /// <summary>
    /// 获取GameObject的完整路径（用于调试）
    /// </summary>
    private string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    /// <summary>
    /// 移除壁纸
    /// </summary>
    private void RemoveWallpaper()
    {
        if (wallpaperInstance != null)
        {
            Destroy(wallpaperInstance);
            wallpaperInstance = null;
            Debug.Log("[StoryModeManager] 壁纸已移除");
        }
    }

    /// <summary>
    /// 隐藏桌宠（进入剧情模式时）
    /// </summary>
    private void HidePet(PetController pet)
    {
        if (pet == null) return;

        // 保存桌宠位置（如果还没保存）
        if (savedPetRectTransform == null)
        {
            SavePetPosition(pet);
        }

        // 查找Pet的根对象（MainCanvas的直接子对象）
        Transform petRoot = FindPetRoot(pet);
        if (petRoot != null)
        {
            // 隐藏Pet的根对象（这会隐藏整个Pet层级）
            petRoot.gameObject.SetActive(false);
            Debug.Log($"[StoryModeManager] 桌宠已隐藏（进入剧情模式），根对象: {petRoot.name}");
        }
        else if (pet.gameObject != null)
        {
            // 如果找不到根对象，直接隐藏PetController所在的GameObject
            pet.gameObject.SetActive(false);
            Debug.Log("[StoryModeManager] 桌宠已隐藏（进入剧情模式，使用直接对象）");
        }
    }

    /// <summary>
    /// 显示桌宠（退出剧情模式时）
    /// </summary>
    private void ShowPet(PetController pet)
    {
        if (pet == null) return;

        // 查找Pet的根对象（MainCanvas的直接子对象）
        Transform petRoot = FindPetRoot(pet);
        if (petRoot != null)
        {
            // 显示Pet的根对象（这会显示整个Pet层级）
            petRoot.gameObject.SetActive(true);
            
            // 恢复桌宠位置（如果之前保存过）
            if (savedPetRectTransform != null)
            {
                RestorePetPosition(pet);
            }
            
            Debug.Log($"[StoryModeManager] 桌宠已显示（退出剧情模式），根对象: {petRoot.name}");
        }
        else if (pet.gameObject != null)
        {
            // 如果找不到根对象，直接显示PetController所在的GameObject
            pet.gameObject.SetActive(true);
            
            // 恢复桌宠位置
            if (savedPetRectTransform != null)
            {
                RestorePetPosition(pet);
            }
            
            Debug.Log("[StoryModeManager] 桌宠已显示（退出剧情模式，使用直接对象）");
        }
    }

    /// <summary>
    /// 查找Pet的根对象（MainCanvas prefab实例）
    /// </summary>
    private Transform FindPetRoot(PetController pet)
    {
        if (pet == null || pet.transform == null) return null;

        // 从PetController向上查找，直到找到MainCanvas（场景根对象）
        Transform current = pet.transform;
        while (current != null && current.parent != null)
        {
            // MainCanvas prefab作为场景根对象，parent为null或场景根
            if (current.name == "MainCanvas")
            {
                // 找到了MainCanvas prefab实例，这就是Pet的根对象
                return current;
            }
            current = current.parent;
        }

        // 如果向上查找到根对象（parent == null），且名称不是MainCanvas
        // 可能是PetController在MainCanvas的子对象下
        // 返回包含MainCanvas的根对象
        current = pet.transform;
        while (current.parent != null)
        {
            current = current.parent;
        }
        
        // 如果根对象名称是MainCanvas，返回它
        if (current.name == "MainCanvas")
        {
            return current;
        }

        return null;
    }

    /// <summary>
    /// 显示退出菜单
    /// </summary>
    private void ShowExitMenu()
    {
        if (StoryModeUI.Instance != null)
        {
            StoryModeUI.Instance.ShowExitMenu();
        }
        else
        {
            Debug.LogWarning("[StoryModeManager] StoryModeUI未找到，直接退出剧情模式");
            ExitStoryMode();
        }
    }

    /// <summary>
    /// 检查是否在剧情模式中
    /// </summary>
    public bool IsInStoryMode => isInStoryMode;
}

