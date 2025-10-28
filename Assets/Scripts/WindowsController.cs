using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class WindowsController : MonoBehaviour
{
    // 导入Windows API函数
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    // 定义窗口样式常量
    private const int GWL_STYLE = -16;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;

    // 定义SetWindowPos常量
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // 将窗口置于所有非顶层窗口之上
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // DWM所需结构体
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    private IntPtr hWnd;

    void Start()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 仅在Windows独立版中执行
        hWnd = GetActiveWindow();

        // 创建一个全屏透明窗口
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 移除窗口边框和标题栏
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 将窗口置顶，并设置为全屏
        StartCoroutine(SetPositionAfterFrame());
#endif
    }

    private IEnumerator SetPositionAfterFrame()
    {
        // 等待一帧以确保所有初始化完成
        yield return null;
        // 重新应用窗口位置和大小，确保它覆盖整个屏幕并置顶
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);
    }
}



