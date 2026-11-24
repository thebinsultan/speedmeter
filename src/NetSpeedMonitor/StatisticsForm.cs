using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

/// <summary>Compact statistics window: current/peak, totals, graph, adapter info.</summary>
public sealed class StatisticsForm : Form
{
    private readonly Label _current = new() { AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };
    private readonly Label _peak = new() { AutoSize = true };
    private readonly Label _today = new() { AutoSize = true };
    private readonly Label _week = new() { AutoSize = true };
    private readonly Label _month = new() { AutoSize = true };
    private readonly Label _adapter = new() { AutoSize = true };
    private readonly Label _ip = new() { AutoSize = true };
    private readonly Label _type = new() { AutoSize = true };
    private readonly Label _heading = new() { AutoSize = true, Text = "Network activity (last 60 samples)" };
    private readonly SpeedGraphControl _graph = new() { Width = 400, Height = 130 };

    public StatisticsForm()
    {
        Text = "Network Statistics";
        ClientSize = new Size(432, 430);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        DoubleBuffered = true;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16),
            AutoScroll = true
        };
        panel.Controls.Add(_current);
        panel.Controls.Add(_peak);
        panel.Controls.Add(_today);
        panel.Controls.Add(_week);
        panel.Controls.Add(_month);
        panel.Controls.Add(_adapter);
        panel.Controls.Add(_ip);
        panel.Controls.Add(_type);
        panel.Controls.Add(_heading);
        panel.Controls.Add(_graph);
        Controls.Add(panel);

        ApplyTheme(new AppSettings());
    }

    public void ApplyTheme(AppSettings settings)
    {
        bool dark = ThemeHelper.IsDarkMode(settings.Theme);
        var back = ThemeHelper.Background(settings.Theme);
        var fore = ThemeHelper.Foreground(settings.Theme);
        BackColor = back;
        ForeColor = fore;
        foreach (Control c in Controls) c.BackColor = back;
        foreach (var l in new[] { _current, _peak, _today, _week, _month, _adapter, _ip, _type, _heading })
            l.ForeColor = fore;
        _graph.SetTheme(dark);
    }

    /// <summary>Pushes a fresh sample into the window; safe to call when hidden.</summary>
    public void RefreshNow(SpeedSample sample, UsageTracker usage, AppSettings settings)
    {
        try
        {
            bool bits = settings.UseBitsPerSecond;
            _current.Text = $"↓ {UnitFormatter.FormatSpeed(sample.DownloadBytesPerSec, bits)}    " +
                            $"↑ {UnitFormatter.FormatSpeed(sample.UploadBytesPerSec, bits)}";
            _peak.Text = $"Peak:  ↓ {UnitFormatter.FormatSpeed(usage.PeakDownloadBytesPerSec, bits)}    " +
                         $"↑ {UnitFormatter.FormatSpeed(usage.PeakUploadBytesPerSec, bits)}";

            var today = DateOnly.FromDateTime(DateTime.Now);
            var (td, tu) = usage.GetToday(today);
            var (wd, wu) = usage.GetWeek(today);
            var (md, mu) = usage.GetMonth(today);
            _today.Text = $"Today:  ↓ {UnitFormatter.FormatBytes(td)}    ↑ {UnitFormatter.FormatBytes(tu)}";
            _week.Text = $"This week:  ↓ {UnitFormatter.FormatBytes(wd)}    ↑ {UnitFormatter.FormatBytes(wu)}";
            _month.Text = $"This month:  ↓ {UnitFormatter.FormatBytes(md)}    ↑ {UnitFormatter.FormatBytes(mu)}";
            _adapter.Text = $"Adapter: {sample.Adapter?.DisplayName ?? "Disconnected"}";
            _ip.Text = $"Local IP: {sample.Adapter?.IPv4Address ?? "-"}";
            _type.Text = $"Connection: {sample.Adapter?.ConnectionType ?? "-"}";
            _graph.AddSample(sample.DownloadBytesPerSec, sample.UploadBytesPerSec);
        }
        catch { /* ignore transient render errors */ }
    }
}
