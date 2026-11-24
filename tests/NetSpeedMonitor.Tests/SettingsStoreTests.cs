using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void Settings_RoundTrip()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var settings = new AppSettings { RefreshIntervalMs = 2000, UseBitsPerSecond = true, FontSize = 11f };
            SettingsStore.SaveSettings(settings, path);
            var loaded = SettingsStore.LoadSettings(path);
            Assert.Equal(2000, loaded.RefreshIntervalMs);
            Assert.True(loaded.UseBitsPerSecond);
            Assert.Equal(11f, loaded.FontSize);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Settings_InvalidInterval_NormalizedTo1000()
    {
        var s = new AppSettings { RefreshIntervalMs = 999 };
        s.Normalize();
        Assert.Equal(1000, s.RefreshIntervalMs);
    }

    [Fact]
    public void Usage_RoundTrip()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var tracker = new UsageTracker();
            tracker.Record(1234, 567, 10, 5, new DateOnly(2026, 1, 1));
            SettingsStore.SaveUsage(tracker, path);
            var loaded = SettingsStore.LoadUsage(path);
            Assert.Equal(1234, loaded.GetToday(new DateOnly(2026, 1, 1)).Down);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void CorruptFile_ReturnsDefaults()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, "{ not json");
        try
        {
            var loaded = SettingsStore.LoadSettings(path);
            Assert.NotNull(loaded);
            Assert.Equal(1000, loaded.RefreshIntervalMs);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
