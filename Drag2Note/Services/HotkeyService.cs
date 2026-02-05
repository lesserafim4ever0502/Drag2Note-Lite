using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Generic;

namespace Drag2Note.Services
{
    public class HotkeyService : IDisposable
    {
        // Win32 Constants
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        
        // Key Codes
        public const uint VK_J = 0x4A; 
        public const uint VK_K = 0x4B; 

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private HwndSource? _source;
        
        private readonly Dictionary<int, Action> _callbacks = new Dictionary<int, Action>();
        private int _currentIdCounter = 9000;

        public HotkeyService()
        {
        }

        public void Initialize(System.Windows.Window window)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;

            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
        }

        public void Register(uint vk, uint modifiers, Action action)
        {
            int id = _currentIdCounter++;
            if (RegisterHotKey(_windowHandle, id, modifiers, vk))
            {
                _callbacks[id] = action;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register hotkey VK={vk}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_callbacks.TryGetValue(id, out var action))
                {
                    action?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            foreach (var id in _callbacks.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _source?.RemoveHook(HwndHook);
            _source = null;
            _callbacks.Clear();
        }
    }
}
