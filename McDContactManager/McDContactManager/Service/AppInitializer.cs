using System.IO;
using System.Windows;

namespace McDContactManager.Service;

public static class AppInitializer
{
    public static string AppFolderPath { get; private set; }

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
        }
        catch (Exception ex)
        {
            // Hibakezelés, ha kell
            MessageBox.Show($"Nem sikerült létrehozni a mappát: {ex.Message}");
        }
    }
}