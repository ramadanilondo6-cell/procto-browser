using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;

namespace Procto
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        // Virtual Key Codes
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_TAB = 0x09;
        private const int VK_ESCAPE = 0x1B;
        private const int VK_CONTROL = 0x11;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_MENU = 0x12; // Alt
        private const int VK_LMENU = 0xA4; // Left Alt
        private const int VK_RMENU = 0xA5; // Right Alt
        private const int VK_SHIFT = 0x10;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_F1 = 0x70;
        private const int VK_F2 = 0x71;
        private const int VK_F3 = 0x72;
        private const int VK_F4 = 0x73;
        private const int VK_F5 = 0x74;
        private const int VK_F6 = 0x75;
        private const int VK_F7 = 0x76;
        private const int VK_F8 = 0x77;
        private const int VK_F9 = 0x78;
        private const int VK_F10 = 0x79;
        private const int VK_F11 = 0x7A;
        private const int VK_F12 = 0x7B;
        private const int VK_A = 0x41;
        private const int VK_B = 0x42;
        private const int VK_C = 0x43;
        private const int VK_D = 0x44;
        private const int VK_E = 0x45;
        private const int VK_F = 0x46;
        private const int VK_G = 0x47;
        private const int VK_H = 0x48;
        private const int VK_I = 0x49;
        private const int VK_J = 0x4A;
        private const int VK_K = 0x4B;
        private const int VK_L = 0x4C;
        private const int VK_M = 0x4D;
        private const int VK_N = 0x4E;
        private const int VK_O = 0x4F;
        private const int VK_P = 0x50;
        private const int VK_Q = 0x51;
        private const int VK_R = 0x52;
        private const int VK_S = 0x53;
        private const int VK_T = 0x54;
        private const int VK_U = 0x55;
        private const int VK_V = 0x56;
        private const int VK_W = 0x57;
        private const int VK_X = 0x58;
        private const int VK_Y = 0x59;
        private const int VK_Z = 0x5A;
        private const int VK_DELETE = 0x2E;
        private const int VK_LEFT = 0x25;
        private const int VK_RIGHT = 0x27;
        private const int VK_SNAPSHOT = 0x2C;
        private const int VK_SCROLL = 0x91;
        private const int VK_PAUSE = 0x13;
        private const int VK_INSERT = 0x2D;
        private const int VK_HOME = 0x24;
        private const int VK_END = 0x23;
        private const int VK_PRIOR = 0x21;
        private const int VK_NEXT = 0x22;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        // Use GetAsyncKeyState for reliable modifier tracking
        private bool IsAltPressed => (GetAsyncKeyState(VK_MENU) & 0x8000) != 0 ||
                                      (GetAsyncKeyState(VK_LMENU) & 0x8000) != 0 ||
                                      (GetAsyncKeyState(VK_RMENU) & 0x8000) != 0;

        private bool IsCtrlPressed => (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0 ||
                                       (GetAsyncKeyState(VK_LCONTROL) & 0x8000) != 0 ||
                                       (GetAsyncKeyState(VK_RCONTROL) & 0x8000) != 0;

        private bool IsWinPressed => (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 ||
                                      (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;

        private bool IsShiftPressed => (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0 ||
                                        (GetAsyncKeyState(VK_LSHIFT) & 0x8000) != 0 ||
                                        (GetAsyncKeyState(VK_RSHIFT) & 0x8000) != 0;

        public Action<int> OnKeyBlocked;

        public void Enable()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            Log.Information("KeyboardHook enabled");
        }

        public void Disable()
        {
            UnhookWindowsHookEx(_hookID);
            Log.Information("KeyboardHook disabled");
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;

                if (isKeyDown)
                {
                    // Get current modifier key states using GetAsyncKeyState
                    bool alt = IsAltPressed;
                    bool ctrl = IsCtrlPressed;
                    bool win = IsWinPressed;
                    bool shift = IsShiftPressed;

                    // Block Windows key (LWIN/RWIN)
                    if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                    {
                        Log.Debug("Blocked Windows key");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Alt+Tab
                    if (alt && vkCode == VK_TAB)
                    {
                        Log.Debug("Blocked Alt+Tab");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Alt+F4
                    if (alt && vkCode == VK_F4)
                    {
                        Log.Debug("Blocked Alt+F4");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Win+Tab (Task View)
                    if (win && vkCode == VK_TAB)
                    {
                        Log.Debug("Blocked Win+Tab");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Alt+Escape
                    if (alt && vkCode == VK_ESCAPE)
                    {
                        Log.Debug("Blocked Alt+Escape");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+E (File Explorer)
                    if (win && vkCode == VK_E)
                    {
                        Log.Debug("Blocked Win+E");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+R (Run)
                    if (win && vkCode == VK_R)
                    {
                        Log.Debug("Blocked Win+R");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+D (Show Desktop)
                    if (win && vkCode == VK_D)
                    {
                        Log.Debug("Blocked Win+D");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+M (Minimize All)
                    if (win && vkCode == VK_M)
                    {
                        Log.Debug("Blocked Win+M");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+I (Settings)
                    if (win && vkCode == VK_I)
                    {
                        Log.Debug("Blocked Win+I");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+X (Quick Link Menu)
                    if (win && vkCode == VK_X)
                    {
                        Log.Debug("Blocked Win+X");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+A (Action Center)
                    if (win && vkCode == VK_A)
                    {
                        Log.Debug("Blocked Win+A");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+C (Cortana)
                    if (win && vkCode == VK_C)
                    {
                        Log.Debug("Blocked Win+C");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+S (Search)
                    if (win && vkCode == VK_S)
                    {
                        Log.Debug("Blocked Win+S");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+L (Lock)
                    if (win && vkCode == VK_L)
                    {
                        Log.Debug("Blocked Win+L");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+P (Project)
                    if (win && vkCode == VK_P)
                    {
                        Log.Debug("Blocked Win+P");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+V (Clipboard)
                    if (win && vkCode == VK_V)
                    {
                        Log.Debug("Blocked Win+V");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+T (Taskbar)
                    if (win && vkCode == VK_T)
                    {
                        Log.Debug("Blocked Win+T");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+B (System Tray)
                    if (win && vkCode == VK_B)
                    {
                        Log.Debug("Blocked Win+B");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+H (Share)
                    if (win && vkCode == VK_H)
                    {
                        Log.Debug("Blocked Win+H");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Windows+K (Connect)
                    if (win && vkCode == VK_K)
                    {
                        Log.Debug("Blocked Win+K");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Ctrl+Shift+Esc (Task Manager)
                    if (ctrl && shift && vkCode == VK_ESCAPE)
                    {
                        Log.Debug("Blocked Ctrl+Shift+Esc");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Ctrl+Alt+Delete (note: this won't fully work, but we try)
                    if (ctrl && alt && vkCode == VK_DELETE)
                    {
                        Log.Debug("Blocked Ctrl+Alt+Delete");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Browser shortcuts - block if Ctrl is pressed
                    if (ctrl)
                    {
                        // Ctrl+R (reload)
                        if (vkCode == VK_R)
                        {
                            Log.Debug("Blocked Ctrl+R");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+P (print)
                        if (vkCode == VK_P)
                        {
                            Log.Debug("Blocked Ctrl+P");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+S (save)
                        if (vkCode == VK_S)
                        {
                            Log.Debug("Blocked Ctrl+S");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+V (paste) - block clipboard paste
                        if (vkCode == VK_V)
                        {
                            Log.Debug("Blocked Ctrl+V");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+C (copy)
                        if (vkCode == VK_C)
                        {
                            Log.Debug("Blocked Ctrl+C");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+X (cut)
                        if (vkCode == VK_X)
                        {
                            Log.Debug("Blocked Ctrl+X");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+A (select all)
                        if (vkCode == VK_A)
                        {
                            Log.Debug("Blocked Ctrl+A");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Z (undo)
                        if (vkCode == VK_Z)
                        {
                            Log.Debug("Blocked Ctrl+Z");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Y (redo)
                        if (vkCode == VK_Y)
                        {
                            Log.Debug("Blocked Ctrl+Y");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+N (new window)
                        if (vkCode == VK_N)
                        {
                            Log.Debug("Blocked Ctrl+N");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+T (new tab)
                        if (vkCode == VK_T)
                        {
                            Log.Debug("Blocked Ctrl+T");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+W (close tab)
                        if (vkCode == VK_W)
                        {
                            Log.Debug("Blocked Ctrl+W");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+O (open file)
                        if (vkCode == VK_O)
                        {
                            Log.Debug("Blocked Ctrl+O");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+I (developer tools - inspect)
                        if (vkCode == VK_I)
                        {
                            Log.Debug("Blocked Ctrl+I");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+U (view source)
                        if (vkCode == VK_U)
                        {
                            Log.Debug("Blocked Ctrl+U");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+K (search)
                        if (vkCode == VK_K)
                        {
                            Log.Debug("Blocked Ctrl+K");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+L (address bar)
                        if (vkCode == VK_L)
                        {
                            Log.Debug("Blocked Ctrl+L");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+D (bookmark)
                        if (vkCode == VK_D)
                        {
                            Log.Debug("Blocked Ctrl+D");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+H (history)
                        if (vkCode == VK_H)
                        {
                            Log.Debug("Blocked Ctrl+H");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+J (downloads)
                        if (vkCode == VK_J)
                        {
                            Log.Debug("Blocked Ctrl+J");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+F (find)
                        if (vkCode == VK_F)
                        {
                            Log.Debug("Blocked Ctrl+F");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+G (find next)
                        if (vkCode == VK_G)
                        {
                            Log.Debug("Blocked Ctrl+G");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+B (bookmarks bar)
                        if (vkCode == VK_B)
                        {
                            Log.Debug("Blocked Ctrl+B");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+M (mute)
                        if (vkCode == VK_M)
                        {
                            Log.Debug("Blocked Ctrl+M");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+I (dev tools)
                        if (shift && vkCode == VK_I)
                        {
                            Log.Debug("Blocked Ctrl+Shift+I");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+J (dev tools console)
                        if (shift && vkCode == VK_J)
                        {
                            Log.Debug("Blocked Ctrl+Shift+J");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+C (inspect element)
                        if (shift && vkCode == VK_C)
                        {
                            Log.Debug("Blocked Ctrl+Shift+C");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+R (hard reload)
                        if (shift && vkCode == VK_R)
                        {
                            Log.Debug("Blocked Ctrl+Shift+R");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+P (print)
                        if (shift && vkCode == VK_P)
                        {
                            Log.Debug("Blocked Ctrl+Shift+P");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+N (incognito)
                        if (shift && vkCode == VK_N)
                        {
                            Log.Debug("Blocked Ctrl+Shift+N");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+T (reopen closed tab)
                        if (shift && vkCode == VK_T)
                        {
                            Log.Debug("Blocked Ctrl+Shift+T");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Shift+Tab (previous tab)
                        if (shift && vkCode == VK_TAB)
                        {
                            Log.Debug("Blocked Ctrl+Shift+Tab");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }

                        // Ctrl+Tab (next tab)
                        if (vkCode == VK_TAB)
                        {
                            Log.Debug("Blocked Ctrl+Tab");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }
                    }

                    // Block Alt+Left/Right (browser navigation)
                    if (alt)
                    {
                        if (vkCode == VK_LEFT || vkCode == VK_RIGHT)
                        {
                            Log.Debug("Blocked Alt+Arrow navigation");
                            OnKeyBlocked?.Invoke(vkCode);
                            return (IntPtr)1;
                        }
                    }

                    // Block F5 (refresh)
                    if (vkCode == VK_F5)
                    {
                        Log.Debug("Blocked F5");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F12 (dev tools)
                    if (vkCode == VK_F12)
                    {
                        Log.Debug("Blocked F12");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F1 (help)
                    if (vkCode == VK_F1)
                    {
                        Log.Debug("Blocked F1");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F2 (rename)
                    if (vkCode == VK_F2)
                    {
                        Log.Debug("Blocked F2");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F3 (search)
                    if (vkCode == VK_F3)
                    {
                        Log.Debug("Blocked F3");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F6 (cycle panes)
                    if (vkCode == VK_F6)
                    {
                        Log.Debug("Blocked F6");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F10 (menu)
                    if (vkCode == VK_F10)
                    {
                        Log.Debug("Blocked F10");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block F11 (fullscreen)
                    if (vkCode == VK_F11)
                    {
                        Log.Debug("Blocked F11");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block PrintScreen
                    if (vkCode == VK_SNAPSHOT)
                    {
                        Log.Debug("Blocked PrintScreen");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block ScrollLock
                    if (vkCode == VK_SCROLL)
                    {
                        Log.Debug("Blocked ScrollLock");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Pause/Break
                    if (vkCode == VK_PAUSE)
                    {
                        Log.Debug("Blocked Pause");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Insert
                    if (vkCode == VK_INSERT)
                    {
                        Log.Debug("Blocked Insert");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Delete
                    if (vkCode == VK_DELETE)
                    {
                        Log.Debug("Blocked Delete");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Home
                    if (vkCode == VK_HOME)
                    {
                        Log.Debug("Blocked Home");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block End
                    if (vkCode == VK_END)
                    {
                        Log.Debug("Blocked End");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Page Up
                    if (vkCode == VK_PRIOR)
                    {
                        Log.Debug("Blocked PageUp");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }

                    // Block Page Down
                    if (vkCode == VK_NEXT)
                    {
                        Log.Debug("Blocked PageDown");
                        OnKeyBlocked?.Invoke(vkCode);
                        return (IntPtr)1;
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }
}
