using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 泡泡消除小游戏
/// </summary>
public class BubblePopMiniGame : MiniGameBase
{
    [Header("游戏配置（从PetProfileSO读取）")]
    [Tooltip("是否使用PetProfileSO的配置（推荐）")]
    [SerializeField] private bool useProfileConfig = true;
    
    [Header("本地配置（仅当useProfileConfig=false时使用）")]
    [Tooltip("游戏时长（秒）")]
    [SerializeField] private float gameDuration = 30f;
    [Tooltip("泡泡初始数量")]
    [SerializeField] private int initialBubbleCount = 15;
    [Tooltip("泡泡最大数量")]
    [SerializeField] private int maxBubbleCount = 25;
    [Tooltip("泡泡生成间隔（秒）")]
    [SerializeField] private float bubbleSpawnInterval = 1.5f;
    [Tooltip("泡泡大小范围")]
    [SerializeField] private Vector2 bubbleSizeRange = new Vector2(50f, 120f);
    [Tooltip("泡泡向上移动速度（像素/秒）")]
    [SerializeField] private float bubbleMoveSpeed = 50f;
    [Tooltip("泡泡超出距离后消失（像素）")]
    [SerializeField] private float bubbleDestroyDistance = 200f;
    [Tooltip("泡泡颜色列表")]
    [SerializeField] private Color[] bubbleColors = new Color[]
    {
        new Color(1f, 0.5f, 0.5f, 0.8f), // 红色
        new Color(0.5f, 1f, 0.5f, 0.8f), // 绿色
        new Color(0.5f, 0.5f, 1f, 0.8f), // 蓝色
        new Color(1f, 1f, 0.5f, 0.8f),   // 黄色
        new Color(1f, 0.5f, 1f, 0.8f),   // 粉色
        new Color(0.5f, 1f, 1f, 0.8f),   // 青色
    };

    [Header("泡泡预制体和资源")]
    [Tooltip("泡泡预制体（如果为空则动态创建）")]
    [SerializeField] private GameObject bubblePrefab;
    
    [Header("泡泡Image（从PetProfileSO读取）")]
    [Tooltip("泡泡精灵（从PetProfileSO读取，可自定义）")]
    public Sprite bubbleSprite;
    
    [Tooltip("如果bubbleSprite为空，是否创建圆形精灵")]
    [SerializeField] private bool createCircleSpriteIfNull = true;

    [Header("UI引用（自动查找）")]
    private RectTransform gameArea;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI scoreText;
    private int score = 0;
    
    [Header("字体配置（从PetProfileSO读取）")]
    [Tooltip("UI文本字体（从PetProfileSO读取，解决中文显示问题）")]
    public TMP_FontAsset uiFontAsset;

    // 实际使用的配置值（从Profile或本地读取）
    private float actualGameDuration;
    private int actualInitialBubbleCount;
    private int actualMaxBubbleCount;
    private float actualBubbleSpawnInterval;
    private Vector2 actualBubbleSizeRange;
    private Color[] actualBubbleColors;
    private float actualBubbleMoveSpeed;
    private float actualBubbleDestroyDistance;

    private float gameTimer = 0f;
    private float spawnTimer = 0f;
    private List<Bubble> activeBubbles = new List<Bubble>();
    private Coroutine gameCoroutine;
    private Coroutine fadeOutCoroutine;
    private CanvasGroup canvasGroup;

    protected override void OnInitialize()
    {
        gameName = "泡泡消除";
    }

    protected override void OnStartGame()
    {
        // 从PetProfileSO或本地配置读取参数
        LoadGameConfig();
        
        Debug.Log("[BubblePopMiniGame] OnStartGame 开始");
        
        if (uiContainer == null)
        {
            Debug.LogError("[BubblePopMiniGame] UI容器为null！");
            return;
        }

        Debug.Log($"[BubblePopMiniGame] UI容器: {uiContainer.name}");

        // 创建UI
        CreateGameUI();
        
        // 应用配置的字体和泡泡精灵
        ApplyProfileAssets();

        if (uiInstance == null)
        {
            Debug.LogError("[BubblePopMiniGame] UI创建失败！");
            return;
        }

        // 重置游戏状态
        score = 0;
        gameTimer = 0f;
        spawnTimer = 0f;
        activeBubbles.Clear();

        // 强制宠物进入Idle状态
        if (petController != null)
        {
            petController.ForceIdleState();
        }

        // 显示UI - 立即设置为可见，而不是淡入（确保能看到）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("[BubblePopMiniGame] UI已显示");
            // 也可以选择淡入效果
            // StartCoroutine(FadeInUI());
        }
        
        // 关键修复：确保父容器MiniGameContainer的CanvasGroup也是可见的
        if (uiContainer != null)
        {
            CanvasGroup containerCanvasGroup = uiContainer.GetComponent<CanvasGroup>();
            if (containerCanvasGroup != null)
            {
                containerCanvasGroup.alpha = 1f;
                containerCanvasGroup.interactable = true;
                containerCanvasGroup.blocksRaycasts = true;
                Debug.Log($"[BubblePopMiniGame] MiniGameContainer的CanvasGroup已设置为可见 (alpha={containerCanvasGroup.alpha})");
            }
            else
            {
                Debug.LogWarning("[BubblePopMiniGame] MiniGameContainer没有CanvasGroup组件");
            }
        }

        // 等待一帧确保UI完全初始化后再生成泡泡
        StartCoroutine(SpawnInitialBubbles());
    }

    /// <summary>
    /// 等待RectTransform更新
    /// </summary>
    private System.Collections.IEnumerator WaitForRectUpdate()
    {
        yield return null; // 等待一帧让RectTransform计算大小
        
        // 强制更新Canvas
        Canvas.ForceUpdateCanvases();
        
        if (gameArea != null)
        {
            Debug.Log($"[BubblePopMiniGame] gameArea大小更新: width={gameArea.rect.width}, height={gameArea.rect.height}, " +
                     $"sizeDelta: {gameArea.sizeDelta}, anchoredPosition: {gameArea.anchoredPosition}");
        }
    }

    /// <summary>
    /// 生成初始泡泡（延迟一帧确保UI初始化完成）
    /// </summary>
    private System.Collections.IEnumerator SpawnInitialBubbles()
    {
        yield return null; // 等待一帧

        // 强制更新Canvas，确保RectTransform大小正确
        Canvas.ForceUpdateCanvases();
        
        // 再次检查gameArea的大小
        if (gameArea != null)
        {
            Debug.Log($"[BubblePopMiniGame] 准备生成泡泡，gameArea大小: width={gameArea.rect.width}, height={gameArea.rect.height}, " +
                     $"sizeDelta: {gameArea.sizeDelta}, worldSize: {gameArea.rect.size}");
            
            // 如果大小仍然为0，尝试手动计算
            if (gameArea.rect.width <= 0 || gameArea.rect.height <= 0)
            {
                Debug.LogWarning("[BubblePopMiniGame] gameArea大小仍为0，尝试使用屏幕尺寸");
                // 这里会在SpawnBubble中处理
            }
        }

        Debug.Log($"[BubblePopMiniGame] 开始生成初始泡泡，数量: {actualInitialBubbleCount}");
        
        // 生成初始泡泡
        for (int i = 0; i < actualInitialBubbleCount; i++)
        {
            SpawnBubble();
            yield return null; // 每生成一个泡泡等待一帧，避免卡顿
        }

        Debug.Log($"[BubblePopMiniGame] 初始泡泡生成完成，当前泡泡数: {activeBubbles.Count}");

        // 启动游戏协程
        gameCoroutine = StartCoroutine(GameLoop());
    }

    protected override void OnEndGame()
    {
        // 停止游戏协程
        if (gameCoroutine != null)
        {
            StopCoroutine(gameCoroutine);
            gameCoroutine = null;
        }

        // 停止淡出协程（如果正在运行）
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // 清除所有泡泡
        foreach (var bubble in activeBubbles)
        {
            if (bubble != null && bubble.gameObject != null)
            {
                Destroy(bubble.gameObject);
            }
        }
        activeBubbles.Clear();

        // 隐藏UI
        if (canvasGroup != null && uiInstance != null)
        {
            fadeOutCoroutine = StartCoroutine(FadeOutUI());
        }
        else if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
            canvasGroup = null;
        }
    }

    protected override void OnGameUpdate()
    {
        if (!IsActive) return;

        // 更新生成计时器
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= actualBubbleSpawnInterval && activeBubbles.Count < actualMaxBubbleCount)
        {
            SpawnBubble();
            spawnTimer = 0f;
        }

        // 更新所有气泡位置并检查是否需要销毁
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            if (activeBubbles[i] != null)
            {
                activeBubbles[i].UpdateBubble(Time.deltaTime);
                // 如果气泡已经超出距离，自动销毁
                if (activeBubbles[i].ShouldDestroy())
                {
                    if (activeBubbles[i].gameObject != null)
                    {
                        Destroy(activeBubbles[i].gameObject);
                    }
                    activeBubbles.RemoveAt(i);
                }
            }
            else
            {
                activeBubbles.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 创建游戏UI
    /// </summary>
    private void CreateGameUI()
    {
        // 创建主容器
        uiInstance = new GameObject("BubblePopMiniGameUI");
        uiInstance.transform.SetParent(uiContainer, false);

        RectTransform mainRect = uiInstance.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;
        mainRect.anchoredPosition = Vector2.zero;

        canvasGroup = uiInstance.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Image background = uiInstance.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0f); // 完全透明背景，不影响游玩
        // 关键修复：背景Image的raycastTarget设置为false，避免阻挡气泡点击
        background.raycastTarget = false;

        // 创建游戏区域
        GameObject gameAreaObj = new GameObject("GameArea");
        gameAreaObj.transform.SetParent(uiInstance.transform, false);
        gameArea = gameAreaObj.AddComponent<RectTransform>();
        
        // 设置gameArea的RectTransform，确保它占据大部分屏幕
        gameArea.anchorMin = new Vector2(0.1f, 0.2f);
        gameArea.anchorMax = new Vector2(0.9f, 0.9f);
        gameArea.pivot = new Vector2(0.5f, 0.5f);
        gameArea.sizeDelta = Vector2.zero;
        gameArea.anchoredPosition = Vector2.zero;
        gameArea.localScale = Vector3.one;
        
        // 确保gameArea激活
        gameAreaObj.SetActive(true);
        
        // 强制更新Canvas，确保RectTransform大小正确计算
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[BubblePopMiniGame] gameArea创建完成 - anchorMin: {gameArea.anchorMin}, anchorMax: {gameArea.anchorMax}, " +
                 $"sizeDelta: {gameArea.sizeDelta}, rect: {gameArea.rect}");
        
        // 等待一帧让RectTransform更新大小
        StartCoroutine(WaitForRectUpdate());

        // 先创建gameArea（确保它在Hierarchy中排在前面，即更早渲染）
        // 然后创建UI文本（文本会渲染在gameArea之上）
        // 最后创建的气泡会在最上层（因为Hierarchy顺序决定UI渲染顺序）
        
        // 创建UI文本（这些文本不应该阻挡点击事件）
        CreateUIText("TimerText", "时间: 30", new Vector2(0.5f, 0.95f), out timerText);
        CreateUIText("ScoreText", "得分: 0", new Vector2(0.5f, 0.05f), out scoreText);
        
        // 确保文本对象在gameArea之后（在Hierarchy中更靠后，以便气泡渲染在最上层）
        if (timerText != null && gameArea != null)
        {
            timerText.transform.SetAsLastSibling();
        }
        if (scoreText != null && gameArea != null)
        {
            scoreText.transform.SetAsLastSibling();
        }
        
        Debug.Log("[BubblePopMiniGame] UI文本已创建，raycastTarget设置为false，不会阻挡气泡点击");
    }

    /// <summary>
    /// 创建UI文本
    /// </summary>
    private void CreateUIText(string name, string initialText, Vector2 anchorPos, out TextMeshProUGUI textComponent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(uiInstance.transform, false);

        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorPos;
        rectTransform.anchorMax = anchorPos;
        rectTransform.sizeDelta = new Vector2(200f, 50f);
        rectTransform.anchoredPosition = Vector2.zero;

        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = initialText;
        textComponent.fontSize = 24f;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        
        // 关键修复：设置raycastTarget为false，避免文本UI阻挡气泡的点击事件
        textComponent.raycastTarget = false;
        
        // 使用配置的字体，如果没有则使用默认字体
        if (uiFontAsset != null)
        {
            textComponent.font = uiFontAsset;
        }
    }

    /// <summary>
    /// 生成泡泡
    /// </summary>
    private void SpawnBubble()
    {
        if (gameArea == null)
        {
            Debug.LogWarning("[BubblePopMiniGame] gameArea为null，无法生成泡泡");
            return;
        }

        GameObject bubbleObj;
        Image image = null; // 在方法作用域开始处声明
        
        if (bubblePrefab != null)
        {
            bubbleObj = Instantiate(bubblePrefab, gameArea);
            // 确保预制体实例也在最后（渲染在最上层）
            bubbleObj.transform.SetAsLastSibling();
        }
        else
        {
            // 动态创建泡泡
            bubbleObj = new GameObject("Bubble");
            bubbleObj.transform.SetParent(gameArea, false);

            RectTransform rectTransform = bubbleObj.AddComponent<RectTransform>();
            float size = Random.Range(actualBubbleSizeRange.x, actualBubbleSizeRange.y);
            
            // 设置RectTransform的anchor和pivot，确保正确显示
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(size, size);
            rectTransform.anchoredPosition = Vector2.zero; // 临时设置为0，后面会更新

            image = bubbleObj.AddComponent<Image>();
            
            // 确保Image Type设置为Simple
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            
            // 使用配置的泡泡精灵，如果没有则创建圆形精灵
            if (bubbleSprite != null)
            {
                image.sprite = bubbleSprite;
                Debug.Log($"[BubblePopMiniGame] 使用配置的泡泡精灵: {bubbleSprite.name}");
            }
            else if (createCircleSpriteIfNull)
            {
                // 创建圆形精灵（可以使用简单的圆形sprite，或者使用默认的sprite）
                image.sprite = CreateCircleSprite((int)size);
                Debug.Log($"[BubblePopMiniGame] 创建了圆形精灵，大小: {size}");
            }
            else
            {
                // 如果没有sprite也没有创建选项，使用默认的sprite
                // 创建一个简单的白色矩形作为备用
                image.sprite = CreateDefaultSprite((int)size);
                Debug.LogWarning("[BubblePopMiniGame] 没有泡泡精灵，使用默认矩形");
            }
            
            // 设置颜色（确保alpha不为0）
            if (actualBubbleColors != null && actualBubbleColors.Length > 0)
            {
                Color bubbleColor = actualBubbleColors[Random.Range(0, actualBubbleColors.Length)];
                // 确保alpha值足够高，至少0.8
                if (bubbleColor.a < 0.8f)
                {
                    bubbleColor.a = 0.8f;
                }
                image.color = bubbleColor;
                Debug.Log($"[BubblePopMiniGame] 设置泡泡颜色: R={bubbleColor.r:F2}, G={bubbleColor.g:F2}, B={bubbleColor.b:F2}, A={bubbleColor.a:F2}");
            }
            else
            {
                // 默认白色，alpha为1
                image.color = new Color(1f, 1f, 1f, 1f);
                Debug.Log("[BubblePopMiniGame] 使用默认白色");
            }
            
            // 确保Image的raycastTarget设置为true，以便接收点击
            image.raycastTarget = true;
            
            // 关键修复：确保气泡对象在Hierarchy中排在最后（即渲染在最上层）
            // 这样即使有其他UI元素，气泡也会渲染在最上面
            bubbleObj.transform.SetAsLastSibling();
        }

        // 随机位置
        RectTransform bubbleRect = bubbleObj.GetComponent<RectTransform>();
        
        // 强制更新RectTransform，确保rect大小正确
        Canvas.ForceUpdateCanvases();
        
        // 确保gameArea的rect大小正确
        float areaWidth = gameArea.rect.width;
        float areaHeight = gameArea.rect.height;
        
        // 如果rect大小为0，尝试使用sizeDelta或屏幕尺寸计算
        if (areaWidth <= 0 || areaHeight <= 0)
        {
            RectTransform areaRect = gameArea;
            if (areaRect != null)
            {
                // 尝试从sizeDelta获取
                Vector2 areaSize = areaRect.sizeDelta;
                if (areaSize.x > 0 && areaSize.y > 0)
                {
                    areaWidth = areaSize.x;
                    areaHeight = areaSize.y;
                }
                else
                {
                    // 使用屏幕尺寸估算（gameArea占屏幕的80%宽度和70%高度）
                    areaWidth = Screen.width * 0.8f;
                    areaHeight = Screen.height * 0.7f;
                }
                Debug.LogWarning($"[BubblePopMiniGame] gameArea rect大小为0，使用估算值: {areaWidth}x{areaHeight}");
            }
        }
        
        // 考虑泡泡大小，确保不会超出边界
        float bubbleSize = bubbleRect.sizeDelta.x;
        float halfBubbleSize = bubbleSize * 0.5f;
        float maxX = Mathf.Max(halfBubbleSize, areaWidth - halfBubbleSize);
        float maxY = Mathf.Max(halfBubbleSize, areaHeight - halfBubbleSize);
        float minX = halfBubbleSize;
        float minY = halfBubbleSize;
        
        // 如果计算出的范围无效，使用默认值
        if (maxX <= minX) maxX = minX + bubbleSize;
        if (maxY <= minY) maxY = minY + bubbleSize;
        
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        
        // 设置位置 - 使用anchoredPosition（相对于gameArea的本地坐标）
        bubbleRect.anchoredPosition = new Vector2(x, y);
        
        // 确保localScale为1，避免缩放问题
        bubbleRect.localScale = Vector3.one;
        
        // 确保GameObject激活
        bubbleObj.SetActive(true);
        
        // 检查Image组件是否正常（如果还没有获取，则获取它）
        if (image == null)
        {
            image = bubbleObj.GetComponent<Image>();
        }
        
        if (image != null)
        {
            // 检查气泡的层级关系
            Transform parent = bubbleObj.transform.parent;
            string parentPath = parent != null ? GetGameObjectPath(parent) : "null";
            
            Debug.Log($"[BubblePopMiniGame] 生成泡泡 - " +
                     $"位置: ({x}, {y}), 大小: {bubbleRect.sizeDelta}, " +
                     $"父对象: {parentPath}, " +
                     $"Sprite: {(image.sprite != null ? image.sprite.name : "null")}, " +
                     $"Color: R={image.color.r:F2}, G={image.color.g:F2}, B={image.color.b:F2}, A={image.color.a:F2}, " +
                     $"ImageType: {image.type}, RaycastTarget: {image.raycastTarget}, Active: {bubbleObj.activeSelf}, " +
                     $"Anchor: min={bubbleRect.anchorMin}, max={bubbleRect.anchorMax}, Pivot: {bubbleRect.pivot}, " +
                     $"LocalScale: {bubbleRect.localScale}, WorldPosition: {bubbleRect.position}");
        }
        else
        {
            Debug.LogError("[BubblePopMiniGame] Image组件为null！");
        }

        // 添加Bubble组件
        Bubble bubble = bubbleObj.AddComponent<Bubble>();
        bubble.Initialize(this, actualBubbleMoveSpeed, actualBubbleDestroyDistance);
        
        activeBubbles.Add(bubble);
        Debug.Log($"[BubblePopMiniGame] 泡泡已添加，当前泡泡总数: {activeBubbles.Count}");
        
        // 验证气泡是否真的在场景中
        if (bubbleObj != null && bubbleObj.activeInHierarchy)
        {
            Debug.Log($"[BubblePopMiniGame] 气泡已激活并在层级中: {bubbleObj.name}");
        }
        else
        {
            Debug.LogWarning($"[BubblePopMiniGame] 警告：气泡可能未激活或不在层级中！");
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
    /// 创建圆形精灵
    /// </summary>
    private Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// 创建默认精灵（备用）
    /// </summary>
    private Sprite CreateDefaultSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // 填充白色
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// 从PetProfileSO或本地配置加载游戏参数
    /// </summary>
    private void LoadGameConfig()
    {
        if (useProfileConfig && petController != null && petController.Profile != null)
        {
            var profile = petController.Profile;
            actualGameDuration = profile.bubbleGameDuration;
            actualInitialBubbleCount = profile.bubbleInitialCount;
            actualMaxBubbleCount = profile.bubbleMaxCount;
            actualBubbleSpawnInterval = profile.bubbleSpawnInterval;
            actualBubbleSizeRange = profile.bubbleSizeRange;
            actualBubbleColors = profile.bubbleColors;
            bubbleSprite = profile.bubbleSprite;
            uiFontAsset = profile.uiFontAsset;
            // 如果Profile中没有这些参数，使用本地配置
            actualBubbleMoveSpeed = bubbleMoveSpeed;
            actualBubbleDestroyDistance = bubbleDestroyDistance;
            
            Debug.Log("[BubblePopMiniGame] 从PetProfileSO加载配置");
        }
        else
        {
            actualGameDuration = gameDuration;
            actualInitialBubbleCount = initialBubbleCount;
            actualMaxBubbleCount = maxBubbleCount;
            actualBubbleSpawnInterval = bubbleSpawnInterval;
            actualBubbleSizeRange = bubbleSizeRange;
            actualBubbleColors = bubbleColors;
            actualBubbleMoveSpeed = bubbleMoveSpeed;
            actualBubbleDestroyDistance = bubbleDestroyDistance;
            
            Debug.Log("[BubblePopMiniGame] 使用本地配置");
        }
        
        Debug.Log($"[BubblePopMiniGame] 游戏配置 - 时长: {actualGameDuration}秒, 初始泡泡: {actualInitialBubbleCount}, 最大泡泡: {actualMaxBubbleCount}");
    }

    /// <summary>
    /// 应用Profile中的资源（字体、泡泡精灵）
    /// </summary>
    private void ApplyProfileAssets()
    {
        if (petController != null && petController.Profile != null)
        {
            var profile = petController.Profile;
            if (profile.bubbleSprite != null)
            {
                bubbleSprite = profile.bubbleSprite;
            }
            if (profile.uiFontAsset != null)
            {
                uiFontAsset = profile.uiFontAsset;
            }
        }
    }

    /// <summary>
    /// 游戏主循环
    /// </summary>
    private IEnumerator GameLoop()
    {
        while (gameTimer < actualGameDuration)
        {
            gameTimer += Time.deltaTime;
            float remainingTime = actualGameDuration - gameTimer;
            
            if (timerText != null)
            {
                timerText.text = $"时间: {Mathf.CeilToInt(remainingTime)}";
            }

            yield return null;
        }

        // 时间到，结束游戏
        OnGameComplete();
    }

    /// <summary>
    /// 泡泡被点击
    /// </summary>
    public void OnBubbleClicked(Bubble bubble)
    {
        if (!IsActive || bubble == null) return;

        score++;
        activeBubbles.Remove(bubble);
        
        if (bubble.gameObject != null)
        {
            Destroy(bubble.gameObject);
        }

        if (scoreText != null)
        {
            scoreText.text = $"得分: {score}";
        }
    }

    /// <summary>
    /// 游戏完成
    /// </summary>
    private void OnGameComplete()
    {
        Debug.Log($"[BubblePopMiniGame] 游戏结束！得分: {score}");
        
        // 可以在这里添加奖励逻辑，比如增加好感度
        if (petController != null)
        {
            // 根据得分给予奖励（可以调整公式）
            float affectionBonus = score * 0.1f;
            petController.Affection += affectionBonus;
            Debug.Log($"[BubblePopMiniGame] 增加好感度: {affectionBonus}");
        }

        EndGame();
    }

    /// <summary>
    /// UI淡入动画
    /// </summary>
    private IEnumerator FadeInUI()
    {
        float duration = 0.3f;
        float timer = 0f;
        
        while (timer < duration && canvasGroup != null && uiInstance != null)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            }
            yield return null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// UI淡出动画
    /// </summary>
    private IEnumerator FadeOutUI()
    {
        float duration = 0.3f;
        float timer = 0f;
        
        // 检查canvasGroup和uiInstance是否仍然有效
        while (timer < duration && canvasGroup != null && uiInstance != null)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            }
            yield return null;
        }
        
        // 再次检查对象是否仍然存在，然后清理
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
        
        // 清理引用
        canvasGroup = null;
        fadeOutCoroutine = null;
    }
}

/// <summary>
/// 泡泡组件
/// </summary>
public class Bubble : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    private BubblePopMiniGame game;
    private RectTransform rectTransform;
    private float moveSpeed;
    private float destroyDistance;
    private float initialY;
    private bool isInitialized = false;

    public void Initialize(BubblePopMiniGame game, float moveSpeed, float destroyDistance)
    {
        this.game = game;
        this.moveSpeed = moveSpeed;
        this.destroyDistance = destroyDistance;
        rectTransform = GetComponent<RectTransform>();
        
        // 记录初始Y位置
        if (rectTransform != null)
        {
            initialY = rectTransform.anchoredPosition.y;
        }
        
        isInitialized = true;
        
        // 添加Button组件以便接收点击事件
        if (GetComponent<Button>() == null)
        {
            Button button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
        }
    }

    /// <summary>
    /// 更新气泡位置（平滑向上移动）
    /// </summary>
    public void UpdateBubble(float deltaTime)
    {
        if (!isInitialized || rectTransform == null) return;

        // 平滑向上移动
        Vector2 currentPos = rectTransform.anchoredPosition;
        currentPos.y += moveSpeed * deltaTime;
        rectTransform.anchoredPosition = currentPos;
    }

    /// <summary>
    /// 检查气泡是否应该被销毁（超出距离）
    /// </summary>
    public bool ShouldDestroy()
    {
        if (!isInitialized || rectTransform == null) return false;

        // 检查是否超出初始位置一定距离
        float currentY = rectTransform.anchoredPosition.y;
        return (currentY - initialY) > destroyDistance;
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (game != null)
        {
            game.OnBubbleClicked(this);
        }
    }
}

