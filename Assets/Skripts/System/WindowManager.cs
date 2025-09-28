// ����: Scripts/System/WindowManager.cs
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class WindowManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiFlag);

    // ��� ����
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_EX_LAYERED = 0x80000;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

    private const uint LWA_COLORKEY = 0x00000001;
    void Awake()
    {
        // ���α׷� ���� �� ���� ���� DPI �ν��� �����մϴ�.
#if !UNITY_EDITOR
        SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
#endif
    }

    void Start()
    {
#if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();

        // â �׵θ� ����
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        
        // 'Layered' ��Ÿ���� �߰��ؾ� SetLayeredWindowAttributes�� �۵��մϴ�.
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        // �÷� Ű ����: RGB(0, 255, 1) ������ �����ϰ� ó���ϰ� Ŭ���� �����ŵ�ϴ�.
        uint colorKey = (1 << 16) | (255 << 8) | 0; // RGB(0, 255, 1) -> 0x00FF01
        SetLayeredWindowAttributes(hWnd, colorKey, 0, LWA_COLORKEY);

        // â ��ġ �� '�׻� ��' �Ӽ� ����
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0x0001 | 0x0002);
#endif
    }
}