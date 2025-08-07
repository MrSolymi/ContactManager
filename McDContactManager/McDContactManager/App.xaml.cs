using System.Configuration;
using System.Data;
using System.Windows;
using McDContactManager.data;
using McDContactManager.Service;

namespace McDContactManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppInitializer.Initialize(); // <- Itt történik a mappa létrehozás
        
        var clientId = EnvLoader.Get("CLIENT_ID");

        if (string.IsNullOrWhiteSpace(clientId))
        {
            MessageBox.Show("Hiányzik vagy hibás a .env fájl!");
            Shutdown();
        }
    }
}