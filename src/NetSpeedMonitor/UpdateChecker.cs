using System.Net.Http.Json;
using System.Text.Json;

namespace NetSpeedMonitor;

/// <summary>Checks GitHub Releases for a newer version. Offline-tolerant, no telemetry.</summary>
public static class UpdateChecker
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/thebinsultan/NetSpeed-Monitor/releases/latest";

    public static async Task<string?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("NetSpeedMonitor/1.0.0");
            using var doc = await http.GetFromJsonAsync<JsonDocument>(LatestReleaseUrl, ct);
            if (doc is null) return null;
            if (!doc.RootElement.TryGetProperty("tag_name", out var tag)) return null;
            string latest = tag.GetString()?.TrimStart('v') ?? string.Empty;
            var current = Application.ProductVersion ?? "1.0.0";
            return Version.TryParse(latest, out var lv) && Version.TryParse(current, out var cv) && lv > cv
                ? latest
                : null;
        }
        catch { return null; }
    }
}
