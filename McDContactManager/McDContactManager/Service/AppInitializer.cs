using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using McDContactManager.Common;

namespace McDContactManager.Service;

public static class AppInitializer
{
    public static string AppFolderPath { get; private set; } = null!;

    public static void Initialize()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        AppFolderPath = Path.Combine(documentsPath, "McDContactManager");

        try
        {
            if (!Directory.Exists(AppFolderPath))
            {
                Directory.CreateDirectory(AppFolderPath);
            }
            
            ConfigManager.EnsureExists();
        }
        catch (Exception ex)
        {
            // Hibakezelés, ha kell
            MessageBox.Show($"Nem sikerült létrehozni a mappát: {ex.Message}");
        }
    }
}