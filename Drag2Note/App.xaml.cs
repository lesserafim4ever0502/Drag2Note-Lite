using System.Windows;
using Drag2Note.Views;

namespace Drag2Note
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var floatingWindow = new FloatingWindow();
            floatingWindow.Show();
        }
    }
}
