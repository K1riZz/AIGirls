using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

/// <summary>
/// 监听并记录Windows桌面全局鼠标点击和键盘按键次数。
/// 这个脚本只在Windows独立版中生效。
/// </summary>
public class DesktopInputTracker : MonoBehaviour
{
    // 合并后的公开静态属性，用于在其他脚本中访问总输入次数
    public static int TotalInputs { get; private set; }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN

    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;

    private static LowLevelProc _keyboardProc;
    private static LowLevelProc _mouseProc;
    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    void OnEnable()
    {
        // 保持委托实例，防止被GC回收
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;
        
        _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
        _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
    }

    void OnDisable()
    {
        // 程序退出或脚本禁用时，务必移除钩子
        UnhookWindowsHookEx(_keyboardHookID);
        UnhookWindowsHookEx(_mouseHookID);
    }
    
    void OnApplicationQuit()
    {
        // 确保在应用退出时也移除钩子
        OnDisable();
    }

    private static IntPtr SetHook(LowLevelProc proc, int hookType)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(hookType, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            TotalInputs++;
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN || wParam == (IntPtr)WM_MBUTTONDOWN)
            {
                TotalInputs++;
            }
        }
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }

    // --- DllImports ---
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

#endif
}
