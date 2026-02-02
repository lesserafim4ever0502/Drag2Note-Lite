using System.Windows;
using Drag2Note.Services;

namespace Drag2Note
{
    public partial class App : Application
    {
        private TrayService _trayService;
        private HotkeyService _hotkeyService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Initialize WindowManager and create the Floating Window
            //    This effectively replaces the previous 'new FloatingWindow().Show()'
            WindowManager.Instance.Initialize();

            // 2. Initialize Tray Service (Icon & Menu)
            _trayService = new TrayService();
            _trayService.Initialize();

            // 3. Initialize Hotkey Service (Alt + J)
            //    We strictly pass the FloatingWindow from our singleton as the handle provider
            if (WindowManager.Instance.FloatingWindow != null)
            {
                _hotkeyService = new HotkeyService(() => WindowManager.Instance.ToggleFloatingWindow());
                
                // Hotkeys need a window handle to register; we use the FloatingWindow's handle.
                // Note: The window must be created (initialized) before this call.
                // We defer initialization slightly to ensure Handle is valid if needed, 
                // but usually after 'new Window()' it's fine if loaded? 
                // Safer to do it after SourceInitialized or verify handle exists.
                // For simplicity here, we assume Show() in WindowManager created the handle.
                _hotkeyService.Initialize(WindowManager.Instance.FloatingWindow);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService?.Dispose();
            base.OnExit(e);
        }
    }
}
