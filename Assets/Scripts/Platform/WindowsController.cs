using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class WindowsController : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

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
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // SetLayeredWindowAttributes
    private const uint LWA_COLORKEY = 0x00000001;

    private IntPtr hWnd;

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        hWnd = GetActiveWindow();

        // 1. 移除窗口边框和标题栏
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 2. 将窗口设置为“分层”模式，这是使用颜色键透明的前提
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        // 3. 设置透明：将纯黑色(crKey=0)作为透明色，并且只使用颜色键(LWA_COLORKEY)
        //    这里的 bAlpha (第二个参数) 必须是一个非零值 (例如255)，以避免整个窗口隐形。
        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_COLORKEY);

        // 4. 将窗口置顶，并设置为全屏
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);
#endif
    }
}