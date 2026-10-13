using System.Runtime.InteropServices;

namespace ControllerMonitor;

public static class Input // WINDOWS ONLY
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private static readonly HashSet<int> PressedKeys = [];
    private static readonly object Lock = new();
    private static readonly HookProc Callback = Hook;
    private static IntPtr HookHandle;

    static Input()
    {
        if (!OperatingSystem.IsWindows()) return;

        HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, Callback, GetModuleHandle(null), 0);
    }

    public static bool IsKeyPressed(string Key)
    {
        if (!OperatingSystem.IsWindows()) return false;

        int VirtualKey = Key.Trim().ToUpperInvariant() switch
        {
            "SHIFT" => 0x10,
            "CTRL" or "CONTROL" => 0x11,
            "ALT" => 0x12,
            "SPACE" => 0x20,
            "ENTER" => 0x0D,
            "TAB" => 0x09,
            "ESC" or "ESCAPE" => 0x1B,
            "BACKSPACE" => 0x08,
            "UP" => 0x26,
            "DOWN" => 0x28,
            "LEFT" => 0x25,
            "RIGHT" => 0x27,
            "CAPSLOCK" => 0x14,
            _ when Key.Trim().Length == 1 && char.IsAsciiLetter(Key.Trim()[0]) => char.ToUpperInvariant(Key.Trim()[0]),
            _ => -1
        };

        lock (Lock) return PressedKeys.Contains(VirtualKey);
    }

    private static IntPtr Hook(int Code, IntPtr WParam, IntPtr LParam)
    {
        if (Code >= 0)
        {
            int Message = unchecked((int)WParam);
            int Key = Marshal.ReadInt32(LParam);

            lock (Lock)
            {
                if (Message is WM_KEYDOWN or WM_SYSKEYDOWN) PressedKeys.Add(Key);
                if (Message is WM_KEYUP or WM_SYSKEYUP) PressedKeys.Remove(Key);
            }
        }

        return CallNextHookEx(HookHandle, Code, WParam, LParam);
    }

    private delegate IntPtr HookProc(int Code, IntPtr WParam, IntPtr LParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int Id, HookProc Callback, IntPtr Module, uint ThreadId);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr Hook, int Code, IntPtr WParam, IntPtr LParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? Name);
}