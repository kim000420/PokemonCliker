// ����: Scripts/System/InputHookManager.cs
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class InputHookManager : MonoBehaviour
{
    // �ܺ� �Է��� �������� �� �ٸ� ��ũ��Ʈ�� �˷��ֱ� ���� �̺�Ʈ
    public static event Action OnGlobalInput;

    // --- Windows API �Լ� �� ��� ���� ---
    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13; // Ű���� ��
    private const int WH_MOUSE_LL = 14;    // ���콺 ��

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_LBUTTONDOWN = 0x0201;

    // --- �� ���� ---
    private static LowLevelProc _keyboardProc;
    private static LowLevelProc _mouseProc;
    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    void OnEnable()
    {
        // Proc�� ������ �÷��ǵǴ� ���� �����ϱ� ���� ��� ������ �Ҵ�
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;

        // �� ��ġ
        _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
        _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
    }

    void OnDisable()
    {
        // �� ���� (������ ���� �� �߿�)
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

    // --- �ݹ� �Լ� (�Է��� �����Ǹ� Windows�� �� �Լ����� ȣ����) ---
    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            // Ű�� ���ȴٴ� ��ȣ�� ����
            OnGlobalInput?.Invoke();
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            // ���콺 ���� ��ư�� ���ȴٴ� ��ȣ�� ����
            OnGlobalInput?.Invoke();
        }
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }
}