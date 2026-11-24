namespace NetSpeedMonitor.Core;

/// <summary>Per-day usage record.</summary>
public sealed class DayUsage
{
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd
    public long DownloadedBytes { get; set; }
    public long UploadedBytes { get; set; }
}

/// <summary>Tracks peaks and daily totals; weekly/monthly are derived from daily buckets.</summary>
public sealed class UsageTracker
{
    public double PeakDownloadBytesPerSec { get; set; }
    public double PeakUploadBytesPerSec { get; set; }
    public Dictionary<string, DayUsage> Daily { get; set; } = new();

    public void Record(long downloadDelta, long uploadDelta,
        double downloadSpeed, double uploadSpeed, DateOnly date)
    {
        if (downloadDelta < 0) downloadDelta = 0;
        if (uploadDelta < 0) uploadDelta = 0;
        if (downloadSpeed > PeakDownloadBytesPerSec) PeakDownloadBytesPerSec = downloadSpeed;
        if (uploadSpeed > PeakUploadBytesPerSec) PeakUploadBytesPerSec = uploadSpeed;

        string key = date.ToString("yyyy-MM-dd");
        if (!Daily.TryGetValue(key, out var day))
        {
            day = new DayUsage { Date = key };
            Daily[key] = day;
        }
        day.DownloadedBytes += downloadDelta;
        day.UploadedBytes += uploadDelta;
        Prune(date);
    }

    public (long Down, long Up) GetToday(DateOnly today)
    {
        string key = today.ToString("yyyy-MM-dd");
        return Daily.TryGetValue(key, out var d) ? (d.DownloadedBytes, d.UploadedBytes) : (0, 0);
    }

    public (long Down, long Up) GetWeek(DateOnly today)
    {
        long down = 0, up = 0;
        for (int i = 0; i < 7; i++)
        {
            var (d, u) = GetToday(today.AddDays(-i));
            down += d; up += u;
        }
        return (down, up);
    }

    public (long Down, long Up) GetMonth(DateOnly today)
    {
        long down = 0, up = 0;
        var first = new DateOnly(today.Year, today.Month, 1);
        for (var d = first; d <= today; d = d.AddDays(1))
        {
            var (dd, uu) = GetToday(d);
            down += dd; up += uu;
        }
        return (down, up);
    }

    public void Reset()
    {
        PeakDownloadBytesPerSec = 0;
        PeakUploadBytesPerSec = 0;
        Daily.Clear();
    }

    private void Prune(DateOnly today)
    {
        // Keep last 62 days so monthly + weekly always work.
        var cutoff = today.AddDays(-62);
        foreach (var key in Daily.Keys.Where(k =>
            DateOnly.TryParseExact(k, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var d) && d < cutoff).ToList())
            Daily.Remove(key);
    }
}
