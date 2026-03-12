using System.Windows.Forms;
using CefSharp;
using Serilog;

namespace Procto
{
    public class KeyboardHandler : IKeyboardHandler
    {
        private readonly bool _allowClipboard;
        private readonly bool _allowPrint;

        public KeyboardHandler(bool allowClipboard, bool allowPrint)
        {
            _allowClipboard = allowClipboard;
            _allowPrint = allowPrint;
            
            Log.Information("KeyboardHandler initialized - Clipboard: {AllowClipboard}, Print: {AllowPrint}", 
                _allowClipboard, _allowPrint);
        }

        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            // Only handle key down events
            if (type != KeyType.RawKeyDown && type != KeyType.KeyDown)
            {
                return false;
            }

            bool ctrl = (modifiers & CefEventFlags.ControlDown) != 0;
            bool alt = (modifiers & CefEventFlags.AltDown) != 0;
            bool shift = (modifiers & CefEventFlags.ShiftDown) != 0;

            // Block Ctrl+V (paste) if clipboard is disabled
            if (!_allowClipboard && ctrl && windowsKeyCode == (int)Keys.V)
            {
                Log.Debug("Blocked Ctrl+V in browser");
                return true;
            }

            // Block Ctrl+P (print) if print is disabled
            if (!_allowPrint && ctrl && windowsKeyCode == (int)Keys.P)
            {
                Log.Debug("Blocked Ctrl+P in browser");
                return true;
            }

            // Block Ctrl+S (save)
            if (ctrl && windowsKeyCode == (int)Keys.S)
            {
                Log.Debug("Blocked Ctrl+S in browser");
                return true;
            }

            // Block Ctrl+R (reload)
            if (ctrl && windowsKeyCode == (int)Keys.R)
            {
                Log.Debug("Blocked Ctrl+R in browser");
                return true;
            }

            // Block F5 (reload)
            if (windowsKeyCode == (int)Keys.F5)
            {
                Log.Debug("Blocked F5 in browser");
                return true;
            }

            // Block F12 (dev tools)
            if (windowsKeyCode == (int)Keys.F12)
            {
                Log.Debug("Blocked F12 in browser");
                return true;
            }

            return false;
        }

        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            // Not used
            return false;
        }
    }
}
