using System.Configuration;
using System.Data;
using System.Windows;
using McDContactManager.data;

namespace McDContactManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var clientId = EnvLoader.Get("CLIENT_ID");

        if (!string.IsNullOrWhiteSpace(clientId)) return;
        
        MessageBox.Show("Hiányzik vagy hibás a .env fájl!");
        Shutdown();
    }
}