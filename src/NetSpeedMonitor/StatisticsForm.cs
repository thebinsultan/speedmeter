using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

public sealed class StatisticsForm : Form
{
    private readonly Label _current = new() { AutoSize = true };
    private readonly Label _peak = new() { AutoSize = true };
    private readonly Label _today = new() { AutoSize = true };
    private readonly Label _week = new() { AutoSize = true };
    private readonly Label _month = new() { AutoSize = true };
    private readonly Label _adapter = new() { AutoSize = true };
    private readonly Label _ip = new() { AutoSize = true };
    private readonly Label _type = new() { AutoSize = true };
    private readonly SpeedGraphControl _graph = new() { Dock = DockStyle.Top, Height = 140 };

    public StatisticsForm()
    {
        Text = "Network Statistics";
        Width = 460;
        Height = 520;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16),
            AutoScroll = true
        };
        panel.Controls.Add(_graph);
        foreach (var l in new[] { _current, _peak, _today, _week, _month, _adapter, _ip, _type })
            panel.Controls.Add(l);
        Controls.Add(panel);
    }

    public void UpdateStats(SpeedSample sample, UsageTracker usage, AppSettings settings)
    {
        bool bits = settings.UseBitsPerSecond;
        _current.Text = $"Current:  ↓ {UnitFormatter.FormatSpeed(sample.DownloadBytesPerSec, bits)}    ↑ {UnitFormatter.FormatSpeed(sample.UploadBytesPerSec, bits)}";
        _peak.Text = $"Peak:  ↓ {UnitFormatter.FormatSpeed(usage.PeakDownloadBytesPerSec, bits)}    ↑ {UnitFormatter.FormatSpeed(usage.PeakUploadBytesPerSec, bits)}";
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
}
