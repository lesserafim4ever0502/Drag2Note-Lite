using System.Windows;
using System.Windows.Input;
using Drag2Note.ViewModels;

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
            this.DragEnter += FloatingWindow_DragEnter;
            this.DragLeave += FloatingWindow_DragLeave;
            this.Drop += FloatingWindow_Drop;
        }

        private void FloatingWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
             if (e.ButtonState == MouseButtonState.Pressed)
             {
                 this.DragMove();
             }
        }

        private void FloatingWindow_DragEnter(object sender, DragEventArgs e)
        {
            _viewModel.OnDragEnter();
        }

        private void FloatingWindow_DragLeave(object sender, DragEventArgs e)
        {
            _viewModel.OnDragLeave();
        }

        private void FloatingWindow_Drop(object sender, DragEventArgs e)
        {
            _viewModel.OnDrop(e.Data);
        }
    }
}
