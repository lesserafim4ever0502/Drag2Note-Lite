using System.Windows;
using System.Windows.Input;
using Drag2Note.ViewModels;
using Drag2Note.Services;

namespace Drag2Note.Views
{
    public partial class FloatingWindow : Window
    {
        private readonly FloatingViewModel _viewModel;

        public FloatingWindow()
        {
            InitializeComponent();
            _viewModel = new FloatingViewModel();
            DataContext = _viewModel;

            this.MouseLeftButtonDown += FloatingWindow_MouseLeftButtonDown;
            // Removed default Hide() on right-click to avoid conflict with Reset logic
            this.DragEnter += FloatingWindow_DragEnter;
            this.Drop += FloatingWindow_Drop;

            // Real-time settings sync
            SettingsService.Instance.SettingsChanged += (s, args) => 
            {
                Dispatcher.Invoke(() => {
                    var settings = SettingsService.Instance.GetSettings();
                    this.Opacity = settings.FloatingOpacity;
                });
            };

            // Set initial opacity
            this.Opacity = SettingsService.Instance.GetSettings().FloatingOpacity;
        }

        private void FloatingWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
             if (e.ButtonState == MouseButtonState.Pressed)
             {
                 this.DragMove();
             }
        }

        private void FloatingWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel.OnDragEnter();
        }

        private void FloatingWindow_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel.OnDragLeave();
        }

        private void FloatingWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel.OnDrop(e.Data);
        }
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.CurrentState != FloatingWindowState.Idle)
            {
                _viewModel.CancelCommand.Execute(null);
                e.Handled = true;
            }
            else
            {
                this.Hide();
            }
        }
    }
}
