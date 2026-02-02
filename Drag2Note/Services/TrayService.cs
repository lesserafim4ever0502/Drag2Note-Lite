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
        private TaskbarIcon _trayIcon;

        public ICommand ToggleFloatingWindowCommand { get; }
        public ICommand ExitCommand { get; }

        public TrayService()
        {
            ToggleFloatingWindowCommand = new RelayCommand(ToggleFloatingWindow);
            ExitCommand = new RelayCommand(ExitApplication);
        }

        public void Initialize()
        {
            // Find resource defined in TrayIcon.xaml
            var resource = Application.Current.FindResource("TrayIcon");
            if (resource is TaskbarIcon icon)
            {
                _trayIcon = icon;
                _trayIcon.DataContext = this;
            }
        }

        private void ToggleFloatingWindow()
        {
            WindowManager.Instance.ToggleFloatingWindow();
        }

        private void ExitApplication()
        {
            _trayIcon?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
