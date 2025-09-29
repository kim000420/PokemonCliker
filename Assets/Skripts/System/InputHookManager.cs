// 파일: Scripts/System/InputHookManager.cs
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class InputHookManager : MonoBehaviour
{
    // 외부 입력을 감지했을 때 다른 스크립트에 알려주기 위한 이벤트
    public static event Action OnGlobalInput;

    // --- Windows API 함수 및 상수 선언 ---
    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13; // 키보드 훅
    private const int WH_MOUSE_LL = 14;    // 마우스 훅

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_LBUTTONDOWN = 0x0201;

    // --- 훅 관리 ---
    private static LowLevelProc _keyboardProc;
    private static LowLevelProc _mouseProc;
    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    void OnEnable()
    {
        // Proc이 가비지 컬렉션되는 것을 방지하기 위해 멤버 변수에 할당
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;

        // 훅 설치
        _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
        _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
    }

    void OnDisable()
    {
        // 훅 해제 (게임이 꺼질 때 중요)
        UnhookWindowsHookEx(_keyboardHookID);
        UnhookWindowsHookEx(_mouseHookID);
    }

    private IntPtr SetHook(LowLevelProc proc, int hookID)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(hookID, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    // --- 콜백 함수 (입력이 감지되면 Windows가 이 함수들을 호출함) ---
    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            // 키가 눌렸다는 신호를 보냄
            OnGlobalInput?.Invoke();
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            // 마우스 왼쪽 버튼이 눌렸다는 신호를 보냄
            OnGlobalInput?.Invoke();
        }
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }
}