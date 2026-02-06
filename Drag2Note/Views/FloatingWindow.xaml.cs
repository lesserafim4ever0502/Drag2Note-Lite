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
                 // Auto-snap to screen edges on release
                 SnapToEdges();
             }
        }

        private void SnapToEdges()
        {
            // Get screen work area (excludes taskbar)
            // Note: This logic primarily supports the primary screen. 
            // For multi-monitor, explicit screen detection would be needed, 
            // but WorkArea often defaults to the screen where the window is mostly located in newer .NET versions or acts on primary.
            // For a "Lite" tool, snapping to primary work area is the expected baseline.
            var workArea = SystemParameters.WorkArea;
            double tolerance = 15.0;

            // Horizontal Snap & Clamp
            if (this.Left < workArea.Left + tolerance) 
            {
                this.Left = workArea.Left;
            }
            else if (this.Left + this.Width > workArea.Right - tolerance)
            {
                this.Left = workArea.Right - this.Width;
            }
            
            // Vertical Snap & Clamp
            if (this.Top < workArea.Top + tolerance)
            {
                this.Top = workArea.Top;
            }
            else if (this.Top + this.Height > workArea.Bottom - tolerance)
            {
                this.Top = workArea.Bottom - this.Height;
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
