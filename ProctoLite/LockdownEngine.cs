using System;
using System.Collections.Generic;
using Serilog;

namespace ProctoLite
{
    public class LockdownEngine
    {
        private KeyboardHook _keyboardHook;
        private ProcessMonitor _processMonitor;
        private readonly List<string> _forbiddenProcesses;
        private readonly string _quitPassword;
        private readonly bool _allowClipboard;
        private readonly bool _allowPrint;
        private readonly string _configKeyHash;

        public bool IsActive { get; private set; } = false;

        public Action<string> ProcessViolationDetected;
        public Action ProcessThresholdReached;

        public LockdownEngine(
            List<string> forbiddenProcesses,
            string quitPassword,
            bool allowClipboard,
            bool allowPrint,
            string configKeyHash)
        {
            _forbiddenProcesses = forbiddenProcesses ?? new List<string>();
            _quitPassword = quitPassword;
            _allowClipboard = allowClipboard;
            _allowPrint = allowPrint;
            _configKeyHash = configKeyHash;

            _keyboardHook = new KeyboardHook();

            Log.Information("LockdownEngine initialized");
            Log.Information("  - Forbidden Processes: {Count}", _forbiddenProcesses.Count);
            Log.Information("  - Clipboard Allowed: {AllowClipboard}", _allowClipboard);
            Log.Information("  - Print Allowed: {AllowPrint}", _allowPrint);
        }

        public void Enable()
        {
            Log.Information("LockdownEngine enabling...");

            // Enable keyboard hook
            _keyboardHook.OnKeyBlocked += (keyCode) =>
            {
                Log.Debug("Keyboard hook blocked key code: {KeyCode}", keyCode);
            };
            _keyboardHook.Enable();

            // Enable process monitor if there are forbidden processes
            if (_forbiddenProcesses.Count > 0)
            {
                _processMonitor = new ProcessMonitor(_forbiddenProcesses);
                _processMonitor.OnViolationDetected += (message) =>
                {
                    ProcessViolationDetected?.Invoke(message);
                };
                _processMonitor.OnThresholdReached += () =>
                {
                    ProcessThresholdReached?.Invoke();
                };
                _processMonitor.Start();
            }

            // Set screen saver running flag to disable Ctrl+Alt+Del and task manager
            // Note: This is a basic approach; full Ctrl+Alt+Del blocking requires driver
            NativeMethods.SystemParametersInfo(
                NativeMethods.SPI_SETSCREENSAVERRUNNING,
                1,
                IntPtr.Zero,
                0);

            IsActive = true;
            Log.Information("LockdownEngine enabled successfully");
        }

        public void Disable()
        {
            Log.Information("LockdownEngine disabling...");

            _keyboardHook?.Disable();
            _processMonitor?.Stop();

            // Restore screen saver running flag
            NativeMethods.SystemParametersInfo(
                NativeMethods.SPI_SETSCREENSAVERRUNNING,
                0,
                IntPtr.Zero,
                0);

            IsActive = false;
            Log.Information("LockdownEngine disabled");
        }
    }

    internal static class NativeMethods
    {
        public const int SPI_SETSCREENSAVERRUNNING = 97;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uAction, int uParam, IntPtr lpvParam, int fuWinIni);
    }
}
