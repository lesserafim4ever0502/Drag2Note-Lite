using System.Windows;
using System.Windows.Input;

namespace Drag2Note.Views
{
    public partial class FloatingWindow : Window
    {
        public FloatingWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += FloatingWindow_MouseLeftButtonDown;
        }

        private void FloatingWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
