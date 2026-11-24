using System.Net.Http.Json;
using System.Text.Json;

namespace NetSpeedMonitor;

/// <summary>Checks GitHub Releases for a newer version. Offline-tolerant, no telemetry.</summary>
public static class UpdateChecker
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/thebinsultan/NetSpeed-Monitor/releases/latest";

    private const string CurrentVersion = "1.0.0";

    /// <summary>Link shown to the user when a newer release exists.</summary>
    public const string ReleasesPage = "https://github.com/thebinsultan/NetSpeed-Monitor/releases";

    public static async Task<string?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("NetSpeedMonitor/1.0.0");
            using var doc = await http.GetFromJsonAsync<JsonDocument>(LatestReleaseUrl, ct);
            if (doc is null) return null;
            if (!doc.RootElement.TryGetProperty("tag_name", out var tag)) return null;
            string latest = tag.GetString()?.TrimStart('v', 'V') ?? string.Empty;
            string current = Application.ProductVersion ?? CurrentVersion;
            if (!Version.TryParse(latest, out var latestVersion)) return null;
            if (!Version.TryParse(current, out var currentVersion)) currentVersion = new Version(1, 0, 0, 0);
            return latestVersion.CompareTo(currentVersion) > 0 ? latest : null;
        }
        catch { return null; }
    }
}
