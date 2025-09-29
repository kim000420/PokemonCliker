// 파일: Scripts/System/WindowManager.cs (정리된 최종 버전)
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

    // 상수 선언
    private const int GWL_EXSTYLE = -20;

    private const uint WS_EX_LAYERED = 0x80000;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private const uint LWA_COLORKEY = 0x00000001;

    void Start()
    {
        // 에디터에서는 작동하지 않도록 설정
#if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();

        // 'Layered' 스타일을 추가해야 SetLayeredWindowAttributes가 작동합니다.
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        // 컬러 키 설정: RGB(0, 255, 1) 색상을 투명하게 처리하고 클릭도 통과시킵니다.
        uint colorKey = (1 << 16) | (255 << 8) | 0; // RGB(0, 255, 1) -> 0x00FF01
        SetLayeredWindowAttributes(hWnd, colorKey, 0, LWA_COLORKEY);

        // 창을 '항상 위'로 설정합니다. 위치와 크기는 변경하지 않습니다. (SWP_NOMOVE | SWP_NOSIZE)
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0x0001 | 0x0002);
#endif
    }
}