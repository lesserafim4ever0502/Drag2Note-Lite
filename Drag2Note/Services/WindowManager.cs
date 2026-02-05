using System;
using System.Windows;
using Drag2Note.Views;

namespace Drag2Note.Services
{
    public class WindowManager
    {
        private static WindowManager _instance;
        public static WindowManager Instance => _instance ??= new WindowManager();

        public FloatingWindow FloatingWindow { get; private set; }
        public MainWindow MainWindow { get; private set; }
        public SettingsWindow SettingsWindow { get; private set; }

        private WindowManager() { }

        public void Initialize()
        {
            if (FloatingWindow == null)
            {
                FloatingWindow = new FloatingWindow();
                if (SettingsService.Instance.GetSettings().EnableDropZone)
                {
                    FloatingWindow.Show(); 
                }
                else
                {
                    FloatingWindow.Hide();
                }
            }

            SettingsService.Instance.SettingsChanged += (s, e) => 
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    var settings = SettingsService.Instance.GetSettings();
                    if (FloatingWindow != null)
                    {
                        if (settings.EnableDropZone) FloatingWindow.Show();
                        else FloatingWindow.Hide();
                    }
                });
            };
            
            // Ensure MainWindow logic - Lazy load or init hidden? User said Tray Click opens it.
            // Let's create it on demand or on init but hide it.
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                // Don't show initially, wait for tray click
            }
        }

        public void ToggleFloatingWindow()
        {
            if (FloatingWindow == null) return;

            if (FloatingWindow.Visibility == Visibility.Visible)
            {
                FloatingWindow.Hide();
            }
            else
            {
                FloatingWindow.Show();
                FloatingWindow.Activate();
                FloatingWindow.Topmost = true; 
            }
        }

        public void ShowMainWindow()
        {
            if (MainWindow == null) MainWindow = new MainWindow();
            
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
            MainWindow.Focus();
        }

        public void ToggleMainWindow()
        {
            if (MainWindow == null || MainWindow.Visibility != Visibility.Visible || MainWindow.WindowState == WindowState.Minimized)
            {
                ShowMainWindow();
            }
            else
            {
                MainWindow.Hide();
            }
        }

        public void OpenSettings()
        {
            if (SettingsWindow == null)
            {
                SettingsWindow = new SettingsWindow();
                SettingsWindow.Closed += (s, e) => SettingsWindow = null;
                SettingsWindow.Show();
            }
            else
            {
                SettingsWindow.Activate();
                if (SettingsWindow.WindowState == WindowState.Minimized)
                    SettingsWindow.WindowState = WindowState.Normal;
            }
        }
    }
}
