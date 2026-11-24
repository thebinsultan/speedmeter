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
    private readonly ToolStripMenuItem _showHideItem;
    private readonly ToolStripMenuItem _startupItem;
    private StatisticsForm? _statsForm;
    private SpeedSample _lastSample = new();
    private Icon? _trayIcon;
    private int _saveCounter;
    private bool _trayDisposed;
    private bool _startupHidden;

    public TrayApplicationContext(AppSettings settings)
    {
        _settings = settings;
        _usage = SettingsStore.LoadUsage();

        _overlay = new TaskbarOverlayForm(settings);
        _overlay.OverlayClicked += ShowStatistics;
        _overlay.OverlayMoved += OnOverlayMoved;

        _showHideItem = new ToolStripMenuItem("Show/Hide Speed Monitor", null, (_, _) => ToggleOverlay());
        _startupItem = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleStartup())
        { Checked = settings.StartWithWindows, CheckOnClick = false };

        _trayIcon = LoadIcon();
        _tray.Icon = _trayIcon;
        _tray.Text = "NetSpeed Monitor";
        _tray.ContextMenuStrip = BuildMenu();
        _tray.Visible = true;
        _tray.DoubleClick += (_, _) => ShowStatistics();
        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowStatistics(); };

        _timer.Tick += (_, _) => Tick();
        _timer.Interval = settings.RefreshIntervalMs;
        _timer.Start();

        if (!settings.StartMinimized) _overlay.Show();
        else _startupHidden = true; // tray-only until the user shows the overlay
        Tick();
    }

    private static Icon LoadIcon()
    {
        try
        {
            using var stream = typeof(TrayApplicationContext).Assembly
                .GetManifestResourceStream("NetSpeedMonitor.Resources.icon.ico");
            if (stream is not null) return new Icon(stream);
        }
        catch { /* fall through */ }
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "icon.ico");
            if (File.Exists(path)) return new Icon(path);
        }
        catch { /* fall through */ }
        return SystemIcons.Application;
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(_showHideItem);
        menu.Items.Add(new ToolStripMenuItem("Network Statistics", null, (_, _) => ShowStatistics()));
        menu.Items.Add(new ToolStripMenuItem("Settings", null, (_, _) => ShowSettings()));
        menu.Items.Add(new ToolStripMenuItem("Reset Usage Statistics", null, (_, _) => ResetUsage()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripMenuItem("Check for Updates", null, (_, _) => CheckForUpdates()));
        menu.Items.Add(new ToolStripMenuItem("About", null, (_, _) => ShowAbout()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitThread()));
        return menu;
    }

    /// <summary>Persists a dragged position as an offset relative to the default taskbar slot.</summary>
    private void OnOverlayMoved(int actualX, int actualY)
    {
        try
        {
            var home = TaskbarHelper.ComputeOverlayPosition(_overlay.Size, 0, 0);
            _settings.OffsetX = Math.Clamp(actualX - home.X, -2000, 2000);
            _settings.OffsetY = Math.Clamp(actualY - home.Y, -2000, 2000);
            SettingsStore.SaveSettings(_settings);
        }
        catch { /* best effort */ }
    }

    private void ToggleOverlay()
    {
        _settings.ShowOverlay = !_settings.ShowOverlay;
        if (_settings.ShowOverlay) _startupHidden = false;
        SettingsStore.SaveSettings(_settings);
        ApplyVisibility(force: true);
    }

    private void ToggleStartup()
    {
        _settings.StartWithWindows = !_settings.StartWithWindows;
        _startupItem.Checked = _settings.StartWithWindows;
        StartupHelper.SetEnabled(_settings.StartWithWindows);
        SettingsStore.SaveSettings(_settings);
    }

    private void ResetUsage()
    {
        var result = MessageBox.Show("Reset all usage statistics? This cannot be undone.",
            "NetSpeed Monitor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;
        _usage.Reset();
        SettingsStore.SaveUsage(_usage);
        _statsForm?.RefreshNow(_lastSample, _usage, _settings);
    }

    private async void CheckForUpdates()
    {
        var latest = await UpdateChecker.CheckForUpdateAsync();
        MessageBox.Show(
            latest is null
                ? "You are using the latest version (or the update service is unreachable)."
                : $"Version {latest} is available at github.com/thebinsultan/NetSpeed-Monitor/releases.",
            "NetSpeed Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        using var form = new AboutForm();
        form.ShowDialog();
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
            ApplyVisibility(force: false);
            _statsForm?.RefreshNow(_lastSample, _usage, _settings);

            string tooltip = $"↓ {UnitFormatter.FormatSpeed(_lastSample.DownloadBytesPerSec, _settings.UseBitsPerSecond)}  " +
                             $"↑ {UnitFormatter.FormatSpeed(_lastSample.UploadBytesPerSec, _settings.UseBitsPerSecond)}";
            // NotifyIcon.Text is limited to 63 characters.
            _tray.Text = tooltip.Length > 62 ? "NetSpeed Monitor" : tooltip;
        }
        catch
        {
            // Never let a transient adapter/permission error kill the tray loop.
        }
    }

    private void ApplyVisibility(bool force)
    {
        bool visible = _settings.ShowOverlay && !_startupHidden;
        if (_settings.HideWhenDisconnected && !_lastSample.IsConnected) visible = false;
        if (_settings.HideWhenZero &&
            _lastSample.DownloadBytesPerSec <= 0 && _lastSample.UploadBytesPerSec <= 0) visible = false;

        _showHideItem.Text = visible ? "Hide Speed Monitor" : "Show Speed Monitor";

        if (visible)
        {
            bool wasVisible = _overlay.Visible;
            if (!wasVisible) _overlay.Show();
            if (force || !wasVisible) _overlay.RefreshLayout();
        }
        else if (_overlay.Visible)
        {
            _overlay.Hide();
        }
    }

    private void ShowStatistics()
    {
        try
        {
            if (_statsForm is null || _statsForm.IsDisposed)
            {
                _statsForm = new StatisticsForm();
                _statsForm.FormClosed += (_, _) => _statsForm = null;
            }
            _statsForm.RefreshNow(_lastSample, _usage, _settings);
            if (!_statsForm.Visible) _statsForm.Show();
            _statsForm.BringToFront();
            _statsForm.Activate();
        }
        catch { /* ignore UI failures */ }
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
            _startupItem.Checked = _settings.StartWithWindows;
            ApplyVisibility(force: true);
            _statsForm?.ApplyTheme(_settings);
        };
        form.ShowDialog();
    }

    protected override void ExitThreadCore()
    {
        if (!_trayDisposed)
        {
            _trayDisposed = true;
            try { SettingsStore.SaveUsage(_usage); } catch { /* ignore */ }
            try
            {
                _timer.Stop();
                _tray.Visible = false;
                _tray.Dispose();
                _trayIcon?.Dispose();
                _overlay.Close();
                _overlay.Dispose();
                _statsForm?.Close();
            }
            catch { /* ignore shutdown errors */ }
        }
        base.ExitThreadCore();
    }
}
