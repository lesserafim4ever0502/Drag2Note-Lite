using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Drag2Note.Services
{
    public class HotkeyService : IDisposable
    {
        // Win32 Constants
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_J = 0x4A; // J key
        // private const uint VK_Q = 0x51; // Q key

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private HwndSource _source;
        private readonly int _hotkeyId = 9000;
        private readonly Action _onHotkeyTriggered;

        public HotkeyService(Action onHotkeyTriggered)
        {
            _onHotkeyTriggered = onHotkeyTriggered;
        }

        public void Initialize(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;

            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            Register();
        }

        private void Register()
        {
            // Register Alt + J
            bool success = RegisterHotKey(_windowHandle, _hotkeyId, MOD_ALT, VK_J);
            if (!success)
            {
                // Handle failure (log or notify) - simplified for now
                System.Diagnostics.Debug.WriteLine("Failed to register hotkey Alt+J");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
            {
                _onHotkeyTriggered?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey(_windowHandle, _hotkeyId);
            _source?.RemoveHook(HwndHook);
            _source = null;
        }
    }
}
