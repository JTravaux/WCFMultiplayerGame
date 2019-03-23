// Names:   Jordan Travaux & Abel Emun
// Date:    March 18, 2019
// Purpose: Application startup

using System.Windows;

namespace ConcentrationClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) => new MainWindow().Show();
    }
}
