using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
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
        CultureInfo culture = new CultureInfo("hu-HU");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        
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