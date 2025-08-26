using System.IO;
using System.Windows;
using McDContactManager.Common;

namespace McDContactManager.Service;

public static class AppInitializer
{
    public static string? AppFolderPath { get; private set; }

    public static void Initialize()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        AppFolderPath = Path.Combine(documentsPath, "McDContactManager");

        try
        {
            Directory.CreateDirectory(AppFolderPath);
        }
        catch (Exception ex)
        {
            // Hibakezelés, ha kell
            MessageBox.Show($"Nem sikerült létrehozni a mappát: {ex.Message}");
        }
    }
}