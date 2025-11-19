using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 剧情模式UI管理器，处理ESC菜单
/// </summary>
public class StoryModeUI : MonoBehaviour
{
    public static StoryModeUI Instance { get; private set; }

    [Header("UI元素")]
    [Tooltip("退出菜单面板（自动查找或创建）")]
    public GameObject exitMenuPanel;

    [Tooltip("退出按钮")]
    public Button exitButton;

    [Tooltip("取消按钮")]
    public Button cancelButton;

    private CanvasGroup exitMenuCanvasGroup;
    private bool isMenuVisible = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 自动查找或创建UI元素
        InitializeUI();
    }

    void InitializeUI()
    {
        // 如果没有指定退出菜单面板，尝试查找或创建
        if (exitMenuPanel == null)
        {
            // 尝试查找
            exitMenuPanel = GameObject.Find("ExitMenuPanel");
            
            // 如果找不到，创建它
            if (exitMenuPanel == null)
            {
                CreateExitMenu();
            }
        }

        if (exitMenuPanel != null)
        {
            exitMenuCanvasGroup = exitMenuPanel.GetComponent<CanvasGroup>();
            if (exitMenuCanvasGroup == null)
            {
                exitMenuCanvasGroup = exitMenuPanel.AddComponent<CanvasGroup>();
            }

            // 默认隐藏菜单
            exitMenuPanel.SetActive(false);
            isMenuVisible = false;

            // 查找按钮
            if (exitButton == null)
            {
                Transform exitBtnTransform = exitMenuPanel.transform.Find("ExitButton");
                if (exitBtnTransform != null)
                {
                    exitButton = exitBtnTransform.GetComponent<Button>();
                }
            }

            if (cancelButton == null)
            {
                Transform cancelBtnTransform = exitMenuPanel.transform.Find("CancelButton");
                if (cancelBtnTransform != null)
                {
                    cancelButton = cancelBtnTransform.GetComponent<Button>();
                }
            }

            // 绑定按钮事件
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
        }
    }

    /// <summary>
    /// 创建退出菜单UI
    /// </summary>
    private void CreateExitMenu()
    {
        // 查找MainCanvas - 使用多种方式查找
        GameObject mainCanvas = FindMainCanvas();
        if (mainCanvas == null)
        {
            Debug.LogError("[StoryModeUI] 无法创建退出菜单：未找到MainCanvas");
            return;
        }

        // 创建退出菜单面板
        exitMenuPanel = new GameObject("ExitMenuPanel");
        exitMenuPanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform panelRect = exitMenuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // 设置在最上层
        exitMenuPanel.transform.SetAsLastSibling();

        exitMenuCanvasGroup = exitMenuPanel.AddComponent<CanvasGroup>();
        exitMenuCanvasGroup.alpha = 0f;
        exitMenuCanvasGroup.interactable = false;
        exitMenuCanvasGroup.blocksRaycasts = false;

        // 创建半透明背景
        Image background = exitMenuPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.7f);

        // 创建菜单容器
        GameObject menuContainer = new GameObject("MenuContainer");
        menuContainer.transform.SetParent(exitMenuPanel.transform, false);

        RectTransform containerRect = menuContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400f, 200f);
        containerRect.anchoredPosition = Vector2.zero;

        Image containerBg = menuContainer.AddComponent<Image>();
        containerBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // 创建标题文本
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(menuContainer.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(350f, 50f);
        titleRect.anchoredPosition = new Vector2(0f, -30f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "退出剧情模式？";
        titleText.fontSize = 24f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // 创建退出按钮
        exitButton = CreateMenuButton("ExitButton", "退出", new Vector2(-100f, -80f), menuContainer.transform);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        // 创建取消按钮
        cancelButton = CreateMenuButton("CancelButton", "取消", new Vector2(100f, -80f), menuContainer.transform);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);

        Debug.Log("[StoryModeUI] 退出菜单UI已创建");
    }

    /// <summary>
    /// 创建菜单按钮
    /// </summary>
    private Button CreateMenuButton(string name, string text, Vector2 position, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(150f, 40f);
        buttonRect.anchoredPosition = position;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // 创建按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18f;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.raycastTarget = false;

        return button;
    }

    /// <summary>
    /// 查找MainCanvas（使用多种方法）
    /// </summary>
    private GameObject FindMainCanvas()
    {
        // 方法1：通过名称查找
        GameObject mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas != null)
        {
            return mainCanvas;
        }

        // 方法2：查找所有Canvas，选择SortingOrder最高的（通常是主Canvas）
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases != null && canvases.Length > 0)
        {
            Canvas mainCanvasComponent = null;
            int highestSortOrder = int.MinValue;
            
            foreach (Canvas canvas in canvases)
            {
                if (canvas.sortingOrder > highestSortOrder)
                {
                    highestSortOrder = canvas.sortingOrder;
                    mainCanvasComponent = canvas;
                }
            }
            
            if (mainCanvasComponent != null)
            {
                return mainCanvasComponent.gameObject;
            }
            
            // 如果找不到SortingOrder最高的，返回第一个
            return canvases[0].gameObject;
        }

        // 方法3：通过Tag查找（如果有设置Tag）
        GameObject taggedCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        if (taggedCanvas != null)
        {
            return taggedCanvas;
        }

        return null;
    }

    /// <summary>
    /// 显示退出菜单
    /// </summary>
    public void ShowExitMenu()
    {
        // 如果菜单面板不存在，尝试初始化或创建
        if (exitMenuPanel == null)
        {
            InitializeUI();
            
            // 如果初始化后仍然没有菜单面板，尝试创建
            if (exitMenuPanel == null)
            {
                CreateExitMenu();
            }
        }

        if (exitMenuPanel != null && !isMenuVisible)
        {
            exitMenuPanel.SetActive(true);
            exitMenuCanvasGroup.interactable = true;
            exitMenuCanvasGroup.blocksRaycasts = true;
            isMenuVisible = true;

            // 淡入动画
            StartCoroutine(FadeMenu(true));
        }
    }

    /// <summary>
    /// 隐藏退出菜单
    /// </summary>
    public void HideExitMenu()
    {
        if (exitMenuPanel != null && isMenuVisible)
        {
            exitMenuCanvasGroup.interactable = false;
            exitMenuCanvasGroup.blocksRaycasts = false;
            isMenuVisible = false;

            // 淡出动画
            StartCoroutine(FadeMenu(false));
        }
    }

    /// <summary>
    /// 退出按钮点击
    /// </summary>
    private void OnExitButtonClicked()
    {
        Debug.Log("[StoryModeUI] 退出按钮被点击");
        HideExitMenu();

        if (StoryModeManager.Instance != null)
        {
            StoryModeManager.Instance.ExitStoryMode();
        }
    }

    /// <summary>
    /// 取消按钮点击
    /// </summary>
    private void OnCancelButtonClicked()
    {
        Debug.Log("[StoryModeUI] 取消按钮被点击");
        HideExitMenu();
    }

    /// <summary>
    /// 淡入/淡出菜单
    /// </summary>
    private System.Collections.IEnumerator FadeMenu(bool fadeIn)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            exitMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        exitMenuCanvasGroup.alpha = endAlpha;

        if (!fadeIn)
        {
            exitMenuPanel.SetActive(false);
        }
    }
}

