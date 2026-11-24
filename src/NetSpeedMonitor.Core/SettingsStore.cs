using System.Text.Json;

namespace NetSpeedMonitor.Core;

/// <summary>JSON persistence for settings and usage under %LocalAppData%\NetSpeedMonitor.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetSpeedMonitor");

    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static string UsagePath => Path.Combine(DataDirectory, "usage.json");

    public static AppSettings LoadSettings(string? path = null)
    {
        try
        {
            string p = path ?? SettingsPath;
            if (!File.Exists(p)) return new AppSettings();
            string json = File.ReadAllText(p);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void SaveSettings(AppSettings settings, string? path = null)
    {
        settings.Normalize();
        string p = path ?? SettingsPath;
        Directory.CreateDirectory(Path.GetDirectoryName(p)!);
        File.WriteAllText(p, JsonSerializer.Serialize(settings, JsonOptions));
    }

    public static UsageTracker LoadUsage(string? path = null)
    {
        try
        {
            string p = path ?? UsagePath;
            if (!File.Exists(p)) return new UsageTracker();
            string json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<UsageTracker>(json, JsonOptions) ?? new UsageTracker();
        }
        catch
        {
            return new UsageTracker();
        }
    }

    public static void SaveUsage(UsageTracker tracker, string? path = null)
    {
        string p = path ?? UsagePath;
        Directory.CreateDirectory(Path.GetDirectoryName(p)!);
        File.WriteAllText(p, JsonSerializer.Serialize(tracker, JsonOptions));
    }
}
