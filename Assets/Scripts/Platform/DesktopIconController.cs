using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// 控制Windows桌面图标的显示和隐藏
/// </summary>
public class DesktopIconController : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    // ShowWindow命令
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    private static DesktopIconController instance;
    public static DesktopIconController Instance => instance;

    private IntPtr desktopIconWnd;
    private bool iconsVisible = true; // 默认图标是可见的

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 隐藏桌面图标
    /// </summary>
    public void HideDesktopIcons()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        try
        {
            // 查找桌面窗口（Desktop）
            IntPtr desktopWnd = FindWindow("Progman", null);
            if (desktopWnd == IntPtr.Zero)
            {
                Debug.LogWarning("[DesktopIconController] 无法找到桌面窗口");
                return;
            }

            // 查找桌面图标列表视图（ListView）
            desktopIconWnd = FindWindowEx(desktopWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (desktopIconWnd == IntPtr.Zero)
            {
                // 尝试另一种方法
                desktopIconWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                desktopIconWnd = FindWindowEx(desktopIconWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            }

            if (desktopIconWnd != IntPtr.Zero)
            {
                ShowWindow(desktopIconWnd, SW_HIDE);
                iconsVisible = false;
                Debug.Log("[DesktopIconController] 桌面图标已隐藏");
            }
            else
            {
                Debug.LogWarning("[DesktopIconController] 无法找到桌面图标窗口");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DesktopIconController] 隐藏桌面图标时发生错误: {e.Message}");
        }
#else
        Debug.Log("[DesktopIconController] 仅在Windows平台下支持隐藏桌面图标");
#endif
    }

    /// <summary>
    /// 显示桌面图标
    /// </summary>
    public void ShowDesktopIcons()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        try
        {
            if (desktopIconWnd != IntPtr.Zero)
            {
                ShowWindow(desktopIconWnd, SW_SHOW);
                iconsVisible = true;
                Debug.Log("[DesktopIconController] 桌面图标已显示");
            }
            else
            {
                // 如果之前没有找到窗口句柄，尝试重新查找
                IntPtr desktopWnd = FindWindow("Progman", null);
                if (desktopWnd != IntPtr.Zero)
                {
                    desktopIconWnd = FindWindowEx(desktopWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (desktopIconWnd == IntPtr.Zero)
                    {
                        desktopIconWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                        desktopIconWnd = FindWindowEx(desktopIconWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                    }

                    if (desktopIconWnd != IntPtr.Zero)
                    {
                        ShowWindow(desktopIconWnd, SW_SHOW);
                        iconsVisible = true;
                        Debug.Log("[DesktopIconController] 桌面图标已显示");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DesktopIconController] 显示桌面图标时发生错误: {e.Message}");
        }
#else
        Debug.Log("[DesktopIconController] 仅在Windows平台下支持显示桌面图标");
#endif
    }

    /// <summary>
    /// 切换桌面图标显示状态
    /// </summary>
    public void ToggleDesktopIcons()
    {
        if (iconsVisible)
        {
            HideDesktopIcons();
        }
        else
        {
            ShowDesktopIcons();
        }
    }

    void OnDestroy()
    {
        // 确保在销毁时恢复桌面图标
        if (!iconsVisible)
        {
            ShowDesktopIcons();
        }
    }

    void OnApplicationQuit()
    {
        // 确保在退出时恢复桌面图标
        if (!iconsVisible)
        {
            ShowDesktopIcons();
        }
    }
}

