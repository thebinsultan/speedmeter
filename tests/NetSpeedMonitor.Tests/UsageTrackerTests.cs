using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class UsageTrackerTests
{
    [Fact]
    public void Record_AndGetToday()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 1, 15);
        tracker.Record(1000, 500, 100, 50, today);
        tracker.Record(2000, 1000, 200, 100, today);
        var (d, u) = tracker.GetToday(today);
        Assert.Equal(3000, d);
        Assert.Equal(1500, u);
        Assert.Equal(200, tracker.PeakDownloadBytesPerSec);
    }

    [Fact]
    public void Week_AndMonth_SumDailyBuckets()
    {
        var tracker = new UsageTracker();
        var today = new DateOnly(2026, 1, 15);
        tracker.Record(1000, 0, 0, 0, today);
        tracker.Record(2000, 0, 0, 0, today.AddDays(-1));
        tracker.Record(4000, 0, 0, 0, today.AddDays(-10));
        Assert.Equal(3000, tracker.GetWeek(today).Down);
        Assert.Equal(7000, tracker.GetMonth(today).Down);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var tracker = new UsageTracker();
        tracker.Record(1000, 1000, 10, 10, new DateOnly(2026, 1, 1));
        tracker.Reset();
        Assert.Empty(tracker.Daily);
        Assert.Equal(0, tracker.PeakDownloadBytesPerSec);
    }
}
