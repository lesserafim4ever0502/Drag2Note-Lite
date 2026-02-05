using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Drag2Note.Services;

namespace Drag2Note.Services
{
    public class TrayService
    {
        private static TrayService? _instance;
        public static TrayService Instance => _instance ??= new TrayService();

        private TaskbarIcon? _trayIcon;

        // Commands
        public ICommand ToggleFloatingWindowCommand { get; } // For Context Menu
        public ICommand ExitCommand { get; }                 // For Context Menu
        public ICommand OpenMainWindowCommand { get; }       // For Left Click
        public ICommand OpenSettingsCommand { get; }         // For Double Click

        private TrayService()
        {
            ToggleFloatingWindowCommand = new RelayCommand(ToggleFloatingWindow);
            ExitCommand = new RelayCommand(ExitApplication);
            OpenMainWindowCommand = new RelayCommand(OpenMainWindow);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        public void Initialize()
        {
            // Find resource defined in TrayIcon.xaml
            var resource = System.Windows.Application.Current.FindResource("TrayIcon");
            if (resource is TaskbarIcon icon)
            {
                _trayIcon = icon;
                _trayIcon.DataContext = this;
                
                // Bind specific events that might not be in XAML yet
                // However, the cleanest way is to verify TrayIcon.xaml or bind commands there.
                // Assuming we can update TrayIcon.xaml next to bind interactions.
                // But we can also set properties in code if XAML binding is tricky.
                _trayIcon.LeftClickCommand = OpenMainWindowCommand;
                _trayIcon.DoubleClickCommand = OpenSettingsCommand;
            }
        }

        private void ToggleFloatingWindow()
        {
            WindowManager.Instance.ToggleFloatingWindow();
        }
        
        private void OpenMainWindow()
        {
            WindowManager.Instance.ShowMainWindow();
        }
        
        private void OpenSettings()
        {
            WindowManager.Instance.OpenSettings();
        }

        private void ExitApplication()
        {
            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
