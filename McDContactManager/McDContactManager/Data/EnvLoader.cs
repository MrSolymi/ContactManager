using System.IO;

namespace McDContactManager.data;

public static class EnvLoader
{
    private static Dictionary<string, string> _values = new();

    static EnvLoader()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
        if (!File.Exists(path)) return;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith($"#")) continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
                _values[parts[0].Trim()] = parts[1].Trim();
        }
    }

    public static string? Get(string key)
    {
        return _values.TryGetValue(key, out var value) ? value : null;
    }
}