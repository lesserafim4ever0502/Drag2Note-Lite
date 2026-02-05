using CommunityToolkit.Mvvm.ComponentModel;

namespace Drag2Note.Models
{
    public partial class AppSettings : ObservableObject
    {
        // General
        [ObservableProperty]
        private bool _darkMode = false;

        [ObservableProperty]
        private bool _launchAtStartup = false;

        // Main Window
        [ObservableProperty]
        private string _globalMainHotkey = "Ctrl+Alt+K";

        [ObservableProperty]
        private bool _moveCompletedToBottom = true;

        [ObservableProperty]
        private bool _autoHideAtEdge = true;

        // Floating Window
        [ObservableProperty]
        private bool _enableDropZone = true;

        [ObservableProperty]
        private double _floatingOpacity = 1.0;

        [ObservableProperty]
        private string _globalFloatHotkey = "Ctrl+Alt+J";
    }
}
