namespace NetSpeedMonitor.Core;

/// <summary>
/// All user preferences persisted to %LocalAppData%\NetSpeedMonitor\settings.json.
/// </summary>
public sealed class AppSettings
{
    public int RefreshIntervalMs { get; set; } = 1000;
    public bool UseBitsPerSecond { get; set; } = false;
    public bool AutoSelectAdapter { get; set; } = true;
    public string? SelectedAdapterId { get; set; }
    public bool LayoutVertical { get; set; } = true;
    public string FontFamily { get; set; } = "Segoe UI";
    public float FontSize { get; set; } = 9f;
    public string DownloadColor { get; set; } = "#2ECC71";
    public string UploadColor { get; set; } = "#3498DB";
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
    /// <summary>0 = opaque (taskbar-matching), 100 = fully transparent.</summary>
    public int BackgroundTransparency { get; set; } = 0;
    /// <summary>System | Light | Dark</summary>
    public string Theme { get; set; } = "System";
    public bool StartWithWindows { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool HideWhenDisconnected { get; set; } = false;
    public bool HideWhenZero { get; set; } = false;
    public bool EnableUsageTracking { get; set; } = true;
    public bool ShowOverlay { get; set; } = true;

    public void Normalize()
    {
        if (RefreshIntervalMs is not (500 or 1000 or 2000 or 5000))
            RefreshIntervalMs = 1000;
        if (string.IsNullOrWhiteSpace(FontFamily)) FontFamily = "Segoe UI";
        if (FontSize < 6f || FontSize > 24f) FontSize = 9f;
        if (string.IsNullOrWhiteSpace(DownloadColor)) DownloadColor = "#2ECC71";
        if (string.IsNullOrWhiteSpace(UploadColor)) UploadColor = "#3498DB";
        BackgroundTransparency = Math.Clamp(BackgroundTransparency, 0, 90);
        if (Theme is not ("System" or "Light" or "Dark")) Theme = "System";
    }
}
