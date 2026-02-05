using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Drag2Note.ViewModels;

namespace Drag2Note.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
            this.PreviewKeyDown += SettingsWindow_PreviewKeyDown;
        }

        private void SettingsWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                if (vm.IsRecordingMainHotkey || vm.IsRecordingFloatHotkey)
                {
                    e.Handled = true;
                    
                    // Ignore modifiers alone
                    if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                        e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                        e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                        e.Key == Key.System) // Alt is often System
                        return;

                    if (e.Key == Key.Escape)
                    {
                        vm.StopRecording();
                        return;
                    }

                    string hotkey = "";
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) hotkey += "Ctrl + ";
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) hotkey += "Alt + ";
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) hotkey += "Shift + ";
                    
                    hotkey += e.Key.ToString();

                    if (vm.IsRecordingMainHotkey) vm.Settings.GlobalMainHotkey = hotkey;
                    else vm.Settings.GlobalFloatHotkey = hotkey;

                    vm.NotifySettingsChanged();
                    vm.StopRecording();
                }
                else if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.SaveCommand.Execute(null);
            }
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
