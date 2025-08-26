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
        
        ConfigManager.EnsureExists();
        var config = ConfigManager.Load();
        
        // AuthService.Initialize();
        //
        // Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        //
        // var config = ConfigManager.Load();
        
        if (config == null || !KeyValidator.IsValid(config.ClientId))
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
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
            
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        
        AuthService.Initialize();
        
        var main = new MainWindow();
        Current.MainWindow = main;
        main.Show();
        
        //Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }
}