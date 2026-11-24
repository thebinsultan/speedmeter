using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class SettingsStoreTests
{
    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), "NetSpeedMonitorTests", Guid.NewGuid() + ".json");

    [Fact]
    public void Settings_RoundTrip()
    {
        string path = TempPath();
        try
        {
            var settings = new AppSettings
            {
                RefreshIntervalMs = 2000,
                UseBitsPerSecond = true,
                FontSize = 11f,
                Theme = "Dark",
                LayoutVertical = false,
                BackgroundTransparency = 25
            };
            SettingsStore.SaveSettings(settings, path);
            var loaded = SettingsStore.LoadSettings(path);
            Assert.Equal(2000, loaded.RefreshIntervalMs);
            Assert.True(loaded.UseBitsPerSecond);
            Assert.Equal(11f, loaded.FontSize);
            Assert.Equal("Dark", loaded.Theme);
            Assert.False(loaded.LayoutVertical);
            Assert.Equal(25, loaded.BackgroundTransparency);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Theory]
    [InlineData(999)]
    [InlineData(0)]
    [InlineData(-100)]
    public void Settings_InvalidInterval_NormalizedTo1000(int interval)
    {
        var s = new AppSettings { RefreshIntervalMs = interval };
        s.Normalize();
        Assert.Equal(1000, s.RefreshIntervalMs);
    }

    [Fact]
    public void Settings_OutOfRangeValues_Normalized()
    {
        var s = new AppSettings
        {
            FontSize = 99f,
            BackgroundTransparency = 500,
            Theme = "Neon",
            FontFamily = "   "
        };
        s.Normalize();
        Assert.Equal(9f, s.FontSize);
        Assert.Equal(90, s.BackgroundTransparency);
        Assert.Equal("System", s.Theme);
        Assert.Equal("Segoe UI", s.FontFamily);
    }

    [Fact]
    public void Usage_RoundTrip()
    {
        string path = TempPath();
        try
        {
            var tracker = new UsageTracker();
            tracker.Record(1234, 567, 10d, 5d, new DateOnly(2026, 1, 1));
            SettingsStore.SaveUsage(tracker, path);
            var loaded = SettingsStore.LoadUsage(path);
            Assert.Equal(1234L, loaded.GetToday(new DateOnly(2026, 1, 1)).Down);
            Assert.Equal(567L, loaded.GetToday(new DateOnly(2026, 1, 1)).Up);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void CorruptSettingsFile_ReturnsDefaults()
    {
        string path = TempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ this is not json");
        try
        {
            var loaded = SettingsStore.LoadSettings(path);
            Assert.NotNull(loaded);
            Assert.Equal(1000, loaded.RefreshIntervalMs);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void CorruptUsageFile_ReturnsEmptyTracker()
    {
        string path = TempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "[[[[");
        try
        {
            var loaded = SettingsStore.LoadUsage(path);
            Assert.Empty(loaded.Daily);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void MissingFile_ReturnsDefaults()
    {
        var loaded = SettingsStore.LoadSettings(TempPath());
        Assert.Equal(1000, loaded.RefreshIntervalMs);
        Assert.True(loaded.AutoSelectAdapter);
    }
}
