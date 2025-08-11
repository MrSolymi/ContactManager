using System.Windows;
using McDContactManager.Common;
using McDContactManager.Security;
using McDContactManager.Service;
using McDContactManager.View;

namespace McDContactManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppInitializer.Initialize();
        
        AuthService.Initialize();
        
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var config = ConfigManager.Load();
        
        if (config == null || !KeyValidator.IsValid(config.ClientId))
        {
            var activationWindow = new ActivationWindow();
            var result = activationWindow.ShowDialog();
            
            if (result != true)
            {
                Shutdown();
                return;
            }
            
            config = ConfigManager.Load();
            if (config == null || !KeyValidator.IsValid(config.ClientId))
            {
                MessageBox.Show("Az aktiválás nem sikerült.");
                Shutdown();
                return;
            }
        }
        
        var main = new MainWindow();
        Current.MainWindow = main;
        main.Show();
        
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }
}