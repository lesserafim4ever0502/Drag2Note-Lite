using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drag2Note.Models;
using Drag2Note.Services;
using System.Windows.Input;

namespace Drag2Note.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _activePage = "General";

        [ObservableProperty]
        private AppSettings _settings;

        [ObservableProperty]
        private bool _isRecordingMainHotkey;

        [ObservableProperty]
        private bool _isRecordingFloatHotkey;

        public IRelayCommand<string> SetPageCommand { get; }
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CloseCommand { get; }
        public IRelayCommand StartRecordMainHotkeyCommand { get; }
        public IRelayCommand StartRecordFloatHotkeyCommand { get; }

        public SettingsViewModel()
        {
            _settings = SettingsService.Instance.GetSettings();
            
            SetPageCommand = new RelayCommand<string>(page => ActivePage = page ?? "General");
            SaveCommand = new RelayCommand(() => SettingsService.Instance.SaveSettings(Settings));
            CloseCommand = new RelayCommand(() => { /* Handled in Window logic */ });

            StartRecordMainHotkeyCommand = new RelayCommand(() => 
            {
                IsRecordingMainHotkey = true;
                IsRecordingFloatHotkey = false;
            });

            StartRecordFloatHotkeyCommand = new RelayCommand(() => 
            {
                IsRecordingFloatHotkey = true;
                IsRecordingMainHotkey = false;
            });

            // Auto-save when any property in Settings changes
            _settings.PropertyChanged += (s, e) => 
            {
                NotifySettingsChanged();
            };
        }

        public void StopRecording()
        {
            IsRecordingMainHotkey = false;
            IsRecordingFloatHotkey = false;
        }

        // Auto-save on property change for many settings
        partial void OnSettingsChanged(AppSettings value)
        {
            SettingsService.Instance.SaveSettings(value);
        }

        // Individual property change notification for immediate persistence 
        public void NotifySettingsChanged()
        {
             SettingsService.Instance.SaveSettings(Settings);
        }
    }
}
