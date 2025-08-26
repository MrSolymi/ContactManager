using System.IO;
using System.Text.Json;
using System.Windows;
using McDContactManager.Service;

namespace McDContactManager.Common;

public static class ConfigManager
{
    private static string ConfigPath => Path.Combine(AppInitializer.AppFolderPath, "config.json");
    
    public static AppConfig? Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return null;

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Config betöltési hiba: {ex.Message}");
            return null;
        }
    }
    
    public static void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Config mentési hiba: {ex.Message}");
        }
    }
    
    public static void EnsureExists()
    {
        if (File.Exists(ConfigPath)) return;
        
        var defaultConfig = new AppConfig
        {
            ClientId = "",
            LastUsedSenderAddress = ""
        };
        Save(defaultConfig);
    }
}