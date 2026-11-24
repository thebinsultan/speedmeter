using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class UsageTrackerTests
{
    [Fact]
    public void Record_AndGetToday()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 1, 15);
        tracker.Record(1000, 500, 100d, 50d, today);
        tracker.Record(2000, 1000, 200d, 100d, today);
        var (d, u) = tracker.GetToday(today);
        Assert.Equal(3000L, d);
        Assert.Equal(1500L, u);
        Assert.Equal(200d, tracker.PeakDownloadBytesPerSec, 3);
        Assert.Equal(100d, tracker.PeakUploadBytesPerSec, 3);
    }

    [Fact]
    public void Week_AndMonth_SumDailyBuckets()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 1, 15);
        tracker.Record(1000, 0, 0d, 0d, today);
        tracker.Record(2000, 0, 0d, 0d, today.AddDays(-1));
        tracker.Record(4000, 0, 0d, 0d, today.AddDays(-10));
        Assert.Equal(3000L, tracker.GetWeek(today).Down);
        Assert.Equal(7000L, tracker.GetMonth(today).Down);
    }

    [Fact]
    public void NegativeDeltas_AreIgnored()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 1, 2);
        tracker.Record(-500, -500, 0d, 0d, today);
        Assert.Equal(0L, tracker.GetToday(today).Down);
    }

    [Fact]
    public void Prune_KeepsRecentDaysOnly()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 3, 1);
        tracker.Record(10, 10, 0d, 0d, today.AddDays(-90));
        tracker.Record(20, 20, 0d, 0d, today);
        string onlyKey = Assert.Single(tracker.Daily).Key;
        Assert.Equal(today, DateOnly.Parse(onlyKey));
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var tracker = new UsageTracker();
        tracker.Record(1000, 1000, 10d, 10d, new DateOnly(2026, 1, 1));
        tracker.Reset();
        Assert.Empty(tracker.Daily);
        Assert.Equal(0d, tracker.PeakDownloadBytesPerSec);
        Assert.Equal(0d, tracker.PeakUploadBytesPerSec);
    }
}
