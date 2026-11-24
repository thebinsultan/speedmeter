using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AppSettings _settings;
    private readonly NetworkMonitor _monitor = new();
    private readonly UsageTracker _usage;
    private readonly NotifyIcon _tray = new();
    private readonly TaskbarOverlayForm _overlay;
    private readonly System.Windows.Forms.Timer _timer = new();
    private StatisticsForm? _statsForm;
    private SpeedSample _lastSample = new();
    private int _saveCounter;

    public TrayApplicationContext(AppSettings settings)
    {
        _settings = settings;
        _usage = SettingsStore.LoadUsage();

        _overlay = new TaskbarOverlayForm(settings);
        _overlay.OverlayClicked += ShowStatistics;
        _overlay.OverlayMoved += (_, _) => { /* offsets are edited in Settings */ };

        _tray.Icon = LoadIcon();
        _tray.Text = "NetSpeed Monitor";
        _tray.Visible = true;
        _tray.ContextMenuStrip = BuildMenu();
        _tray.DoubleClick += (_, _) => ShowStatistics();
        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowStatistics(); };

        _timer.Tick += (_, _) => Tick();
        _timer.Interval = settings.RefreshIntervalMs;
        _timer.Start();

        if (!settings.StartMinimized)
        {
            _overlay.Show();
            _overlay.Reposition();
        }
        Tick();
    }

    private static Icon LoadIcon()
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "icon.ico");
            if (File.Exists(path)) return new Icon(path);
            using var stream = typeof(TrayApplicationContext).Assembly
                .GetManifestResourceStream("NetSpeedMonitor.Resources.icon.ico");
            if (stream is not null) return new Icon(stream);
        }
        catch { /* fall through */ }
        return SystemIcons.Application;
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        var showHide = new ToolStripMenuItem("Show/Hide Speed Monitor");
        showHide.Click += (_, _) =>
        {
            _settings.ShowOverlay = !_settings.ShowOverlay;
            SettingsStore.SaveSettings(_settings);
            ApplyVisibility();
        };
        var stats = new ToolStripMenuItem("Network Statistics");
        stats.Click += (_, _) => ShowStatistics();
        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (_, _) => ShowSettings();
        var reset = new ToolStripMenuItem("Reset Usage Statistics");
        reset.Click += (_, _) =>
        {
            _usage.Reset();
            SettingsStore.SaveUsage(_usage);
            MessageBox.Show("Usage statistics have been reset.", "NetSpeed Monitor",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        var startup = new ToolStripMenuItem("Start with Windows") { Checked = _settings.StartWithWindows };
        startup.Click += (_, _) =>
        {
            _settings.StartWithWindows = !_settings.StartWithWindows;
            startup.Checked = _settings.StartWithWindows;
            StartupHelper.SetEnabled(_settings.StartWithWindows);
            SettingsStore.SaveSettings(_settings);
        };
        var updates = new ToolStripMenuItem("Check for Updates");
        updates.Click += async (_, _) =>
        {
            var latest = await UpdateChecker.CheckForUpdateAsync();
            MessageBox.Show(
                latest is null ? "You are using the latest version." : $"Version {latest} is available.",
                "NetSpeed Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        var about = new ToolStripMenuItem("About");
        about.Click += (_, _) => new AboutForm().ShowDialog();
        var exit = new ToolStripMenuItem("Exit");
        exit.Click += (_, _) => ExitThread();

        menu.Items.AddRange(new ToolStripItem[]
            { showHide, stats, settingsItem, reset, startup, updates, about, exit });
        return menu;
    }

    private void Tick()
    {
        try
        {
            _lastSample = _monitor.Poll(_settings);
            if (_settings.EnableUsageTracking)
            {
                _usage.Record(_lastSample.DownloadDelta, _lastSample.UploadDelta,
                    _lastSample.DownloadBytesPerSec, _lastSample.UploadBytesPerSec,
                    DateOnly.FromDateTime(DateTime.Now));
                if (++_saveCounter % 10 == 0) SettingsStore.SaveUsage(_usage);
            }
            _overlay.SetSpeeds(_lastSample.DownloadBytesPerSec, _lastSample.UploadBytesPerSec);
            ApplyVisibility();
            _tray.Text = $"↓ {UnitFormatter.FormatSpeed(_lastSample.DownloadBytesPerSec, _settings.UseBitsPerSecond)}  ↑ {UnitFormatter.FormatSpeed(_lastSample.UploadBytesPerSec, _settings.UseBitsPerSec)}";
            _statsForm?.UpdateStats(_lastSample, _usage, _settings);
        }
        catch { /* never crash the tray loop */ }
    }

    private void ApplyVisibility()
    {
        bool visible = _settings.ShowOverlay;
        if (_settings.HideWhenDisconnected && !_lastSample.IsConnected) visible = false;
        if (_settings.HideWhenZero && _lastSample.DownloadBytesPerSec == 0 && _lastSample.UploadBytesPerSec == 0) visible = false;
        if (visible && !_overlay.Visible) { _overlay.Show(); _overlay.Reposition(); }
        if (!visible && _overlay.Visible) _overlay.Hide();
    }

    private void ShowStatistics()
    {
        if (_statsForm is null || _statsForm.IsDisposed)
        {
            _statsForm = new StatisticsForm();
            _statsForm.FormClosed += (_, _) => _statsForm = null;
        }
        _statsForm.UpdateStats(_lastSample, _usage, _settings);
        if (!_statsForm.Visible) _statsForm.Show();
        _statsForm.Activate();
    }

    private void ShowSettings()
    {
        var adapters = _monitor.GetAdapters();
        using var form = new SettingsForm(_settings, adapters);
        form.Saved += () =>
        {
            _timer.Interval = _settings.RefreshIntervalMs;
            _overlay.ApplySettings(_settings);
            StartupHelper.SetEnabled(_settings.StartWithWindows);
            ApplyVisibility();
        };
        form.ShowDialog();
    }

    protected override void ExitThreadCore()
    {
        try { SettingsStore.SaveUsage(_usage); } catch { /* ignore */ }
        _timer.Stop();
        _overlay.Close();
        _tray.Visible = false;
        _tray.Dispose();
        base.ExitThreadCore();
    }
}
