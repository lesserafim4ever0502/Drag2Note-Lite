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

        private WindowManager() { }

        public void Initialize()
        {
            if (FloatingWindow == null)
            {
                FloatingWindow = new FloatingWindow();
                FloatingWindow.Show(); 
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
    }
}
