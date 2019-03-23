using System.Windows;

namespace ConcentrationClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) => new MainWindow().Show();
    }
}
