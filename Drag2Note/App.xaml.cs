using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Drag2Note.Services;

namespace Drag2Note
{
    public partial class App : System.Windows.Application
    {
        private HotkeyService _hotkeyService;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, ev) => LogError(ev.ExceptionObject as Exception);
            DispatcherUnhandledException += (s, ev) => { LogError(ev.Exception); ev.Handled = true; };

            try 
            {
                // 0. Initial theme set (as early as possible to prevent flash)
                var settings = SettingsService.Instance.GetSettings();
                UpdateTheme(settings.DarkMode);

                // 1. Initialize WindowManager
                WindowManager.Instance.Initialize();

                // 2. Initialize Tray Service
                TrayService.Instance.Initialize();

                // 3. Register Hotkeys
                _hotkeyService = new HotkeyService();
                _hotkeyService.Initialize(WindowManager.Instance.FloatingWindow);
                
                RegisterGlobalHotkeys(settings);

                // Listen for setting changes to re-register hotkeys and update theme
                SettingsService.Instance.SettingsChanged += (s, args) => 
                {
                    Dispatcher.Invoke(() => 
                    {
                        RegisterGlobalHotkeys(SettingsService.Instance.GetSettings());
                        UpdateTheme(SettingsService.Instance.GetSettings().DarkMode);
                    });
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                System.Windows.MessageBox.Show($"Startup Error: {ex.Message}");
                Shutdown();
            }
        }

        private void RegisterGlobalHotkeys(Models.AppSettings settings)
        {
            _hotkeyService?.Dispose();
            _hotkeyService = new HotkeyService();
            _hotkeyService.Initialize(WindowManager.Instance.FloatingWindow);

            // Simple parser for Hotkey strings like "Ctrl+Alt+J"
            RegisterHotkeyString(settings.GlobalFloatHotkey, () => WindowManager.Instance.ToggleFloatingWindow());
            RegisterHotkeyString(settings.GlobalMainHotkey, () => WindowManager.Instance.ToggleMainWindow());
        }

        private void RegisterHotkeyString(string hotkey, Action action)
        {
            if (string.IsNullOrEmpty(hotkey)) return;
            
            uint modifiers = 0;
            uint key = 0;

            // Handle both "Ctrl+K" and "Ctrl + K"
            var parts = hotkey.Split(new[] { "+", " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var p = part.Trim().ToLower();
                if (p == "ctrl") modifiers |= 0x0002;
                else if (p == "alt") modifiers |= 0x0001;
                else if (p == "shift") modifiers |= 0x0004;
                else
                {
                    try
                    {
                        // Try to parse the key name (e.g., "K", "F1", "Space")
                        if (Enum.TryParse<Key>(p, true, out var enumKey))
                        {
                            key = (uint)KeyInterop.VirtualKeyFromKey(enumKey);
                        }
                    }
                    catch { }
                }
            }

            if (key != 0)
            {
                _hotkeyService?.Register(key, modifiers, action);
            }
        }

        public void UpdateTheme(bool isDarkMode)
        {
            var themeUri = isDarkMode
                ? new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative);

            // Find existing theme dictionary
            var existingTheme = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("LightTheme.xaml") || d.Source.OriginalString.Contains("DarkTheme.xaml")));

            if (existingTheme != null)
            {
                Resources.MergedDictionaries.Remove(existingTheme);
            }

            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }

        private void LogError(Exception? ex)
        {
            if (ex == null) return;
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {ex.ToString()}{Environment.NewLine}");
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            _hotkeyService?.Dispose();
            base.OnExit(e);
        }
    }
}
