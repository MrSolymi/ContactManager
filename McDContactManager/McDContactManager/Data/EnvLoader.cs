using System.IO;
using System.Windows;
using McDContactManager.Service;

namespace McDContactManager.data;

public static class EnvLoader
{
    private static Dictionary<string, string> _values = new();

    static EnvLoader()
    {
        var appFolder = AppInitializer.AppFolderPath;
        var targetEnvPath = Path.Combine(appFolder, ".env");
        
        // Ha még nincs ott, másoljuk át a projekt gyökeréből
        var sourceEnvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

        try
        {
            if (!File.Exists(targetEnvPath))
            {
                if (File.Exists(sourceEnvPath))
                {
                    File.Copy(sourceEnvPath, targetEnvPath);
                }
                else
                {
                    MessageBox.Show("Nem található a forrás .env fájl a program mappában.");
                    return;
                }
            }

            // Betöltés a célhelyről (Documents...)
            foreach (var line in File.ReadAllLines(targetEnvPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                    _values[parts[0].Trim()] = parts[1].Trim();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($".env betöltési hiba: {ex.Message}");
        }
    }

    public static string? Get(string key)
    {
        return _values.TryGetValue(key, out var value) ? value : null;
    }

}