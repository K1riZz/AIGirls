
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class WindowsController : MonoBehaviour
{
    public static WindowsController Instance { get; private set; }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    // --- 常量定义 ---
    // 窗口样式
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_EX_LAYERED = 0x00080000;

    // SetWindowPos
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_NOMOVE_NOSIZE = SWP_NOMOVE | SWP_NOSIZE;

    // SetLayeredWindowAttributes
    private const uint LWA_COLORKEY = 0x00000001;
    private const uint LWA_ALPHA = 0x00000002;

    private IntPtr hWnd;
    private bool isFullscreenMode = false;
    private uint originalStyle;
    private uint originalExStyle;

    /// <summary>
    /// 检查是否处于全屏模式
    /// </summary>
    public bool IsFullscreenMode => isFullscreenMode;

    void Awake()
    {
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
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        hWnd = GetActiveWindow();

        if (hWnd != IntPtr.Zero)
        {
            // 保存原始窗口样式
            originalStyle = (uint)GetWindowLong(hWnd, GWL_STYLE);
            originalExStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);

            // 初始化桌面模式（透明桌面宠物）
            InitializeDesktopMode();
        }
#endif
    }

    /// <summary>
    /// 初始化桌面模式（透明桌面宠物）
    /// </summary>
    private void InitializeDesktopMode()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (hWnd == IntPtr.Zero) return;

        // 1. 移除窗口边框和标题栏
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 2. 将窗口设置为"分层"模式，这是使用颜色键透明的前提
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        // 3. 设置透明：将纯黑色(crKey=0)作为透明色，并且只使用颜色键(LWA_COLORKEY)
        //    这里的 bAlpha (第二个参数) 必须是一个非零值 (例如255)，以避免整个窗口隐形。
        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_COLORKEY);

        // 4. 将窗口置顶，并设置为全屏
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);

        isFullscreenMode = false; // 桌面模式不是全屏AVG模式
        Debug.Log("[WindowsController] 桌面模式已初始化");
#endif
    }

    /// <summary>
    /// 进入全屏模式（剧情模式）
    /// </summary>
    public void EnterFullscreenMode()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (hWnd == IntPtr.Zero || isFullscreenMode) return;

        Debug.Log("[WindowsController] 进入全屏模式...");

        // 切换到全屏模式，移除透明
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        // 设置不透明（alpha = 255，不使用颜色键）
        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA);

        // 确保窗口全屏并置顶
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);

        isFullscreenMode = true;
        Debug.Log("[WindowsController] 已进入全屏模式");
#endif
    }

    /// <summary>
    /// 退出全屏模式（返回桌面模式）
    /// </summary>
    public void ExitFullscreenMode()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (hWnd == IntPtr.Zero || !isFullscreenMode) return;

        Debug.Log("[WindowsController] 退出全屏模式...");

        // 恢复桌面模式（透明）
        InitializeDesktopMode();

        Debug.Log("[WindowsController] 已退出全屏模式");
#endif
    }
}