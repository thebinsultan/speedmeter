using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

public sealed class SettingsForm : Form
{
    private readonly ComboBox _interval = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _bits = new() { Text = "Display speeds in bits per second (bps)" };
    private readonly CheckBox _autoAdapter = new() { Text = "Automatically select network adapter" };
    private readonly ComboBox _adapter = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _vertical = new() { Text = "Vertical (two-line) layout" };
    private readonly TextBox _fontFamily = new();
    private readonly NumericUpDown _fontSize = new() { Minimum = 6, Maximum = 24, DecimalPlaces = 1, Increment = 0.5M };
    private readonly TextBox _downColor = new();
    private readonly TextBox _upColor = new();
    private readonly NumericUpDown _offsetX = new() { Minimum = -500, Maximum = 500 };
    private readonly NumericUpDown _offsetY = new() { Minimum = -500, Maximum = 500 };
    private readonly NumericUpDown _transparency = new() { Minimum = 0, Maximum = 90 };
    private readonly ComboBox _theme = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _startWindows = new() { Text = "Start with Windows" };
    private readonly CheckBox _startMinimized = new() { Text = "Start minimized" };
    private readonly CheckBox _hideDisconnected = new() { Text = "Hide when disconnected" };
    private readonly CheckBox _hideZero = new() { Text = "Hide when network activity is zero" };
    private readonly CheckBox _tracking = new() { Text = "Enable data usage tracking" };
    private readonly CheckBox _showOverlay = new() { Text = "Show speed monitor on taskbar" };

    public AppSettings Settings { get; private set; }
    public event Action? Saved;

    public SettingsForm(AppSettings settings, IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        Settings = settings;
        Text = "Settings";
        Width = 480;
        Height = 560;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _interval.Items.AddRange(new object[] { "500 ms", "1 s", "2 s", "5 s" });
        _theme.Items.AddRange(new object[] { "System", "Light", "Dark" });
        foreach (var a in adapters) _adapter.Items.Add(a.DisplayName);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16),
            AutoScroll = true
        };
        panel.Controls.Add(new Label { Text = "Refresh interval:", AutoSize = true });
        panel.Controls.Add(_interval);
        panel.Controls.Add(_bits);
        panel.Controls.Add(_autoAdapter);
        panel.Controls.Add(new Label { Text = "Network adapter:", AutoSize = true });
        panel.Controls.Add(_adapter);
        panel.Controls.Add(_vertical);
        panel.Controls.Add(new Label { Text = "Font family:", AutoSize = true });
        panel.Controls.Add(_fontFamily);
        panel.Controls.Add(new Label { Text = "Font size:", AutoSize = true });
        panel.Controls.Add(_fontSize);
        panel.Controls.Add(new Label { Text = "Download color (e.g. #2ECC71):", AutoSize = true });
        panel.Controls.Add(_downColor);
        panel.Controls.Add(new Label { Text = "Upload color (e.g. #3498DB):", AutoSize = true });
        panel.Controls.Add(_upColor);
        panel.Controls.Add(new Label { Text = "Offset X:", AutoSize = true });
        panel.Controls.Add(_offsetX);
        panel.Controls.Add(new Label { Text = "Offset Y:", AutoSize = true });
        panel.Controls.Add(_offsetY);
        panel.Controls.Add(new Label { Text = "Background transparency %:", AutoSize = true });
        panel.Controls.Add(_transparency);
        panel.Controls.Add(new Label { Text = "Theme:", AutoSize = true });
        panel.Controls.Add(_theme);
        panel.Controls.Add(_startWindows);
        panel.Controls.Add(_startMinimized);
        panel.Controls.Add(_hideDisconnected);
        panel.Controls.Add(_hideZero);
        panel.Controls.Add(_tracking);
        panel.Controls.Add(_showOverlay);

        var save = new Button { Text = "Save", Dock = DockStyle.Bottom, Height = 36 };
        save.Click += (_, _) => { Collect(); SettingsStore.SaveSettings(Settings); Saved?.Invoke(); Close(); };
        var cancel = new Button { Text = "Cancel", Dock = DockStyle.Bottom, Height = 36 };
        cancel.Click += (_, _) => Close();

        Controls.Add(panel);
        Controls.Add(save);
        Controls.Add(cancel);
        LoadValues(settings, adapters);
    }

    private void LoadValues(AppSettings s, IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        _interval.SelectedIndex = s.RefreshIntervalMs switch { 500 => 0, 2000 => 2, 5000 => 3, _ => 1 };
        _bits.Checked = s.UseBitsPerSecond;
        _autoAdapter.Checked = s.AutoSelectAdapter;
        _vertical.Checked = s.LayoutVertical;
        _fontFamily.Text = s.FontFamily;
        _fontSize.Value = (decimal)s.FontSize;
        _downColor.Text = s.DownloadColor;
        _upColor.Text = s.UploadColor;
        _offsetX.Value = s.OffsetX;
        _offsetY.Value = s.OffsetY;
        _transparency.Value = s.BackgroundTransparency;
        _theme.SelectedItem = s.Theme;
        _startWindows.Checked = s.StartWithWindows;
        _startMinimized.Checked = s.StartMinimized;
        _hideDisconnected.Checked = s.HideWhenDisconnected;
        _hideZero.Checked = s.HideWhenZero;
        _tracking.Checked = s.EnableUsageTracking;
        _showOverlay.Checked = s.ShowOverlay;
        if (!s.AutoSelectAdapter)
        {
            int idx = adapters.ToList().FindIndex(a => a.Id == s.SelectedAdapterId);
            if (idx >= 0) _adapter.SelectedIndex = idx;
        }
    }

    private void Collect()
    {
        Settings.RefreshIntervalMs = _interval.SelectedIndex switch { 0 => 500, 2 => 2000, 3 => 5000, _ => 1000 };
        Settings.UseBitsPerSecond = _bits.Checked;
        Settings.AutoSelectAdapter = _autoAdapter.Checked;
        Settings.LayoutVertical = _vertical.Checked;
        Settings.FontFamily = _fontFamily.Text;
        Settings.FontSize = (float)_fontSize.Value;
        Settings.DownloadColor = _downColor.Text;
        Settings.UploadColor = _upColor.Text;
        Settings.OffsetX = (int)_offsetX.Value;
        Settings.OffsetY = (int)_offsetY.Value;
        Settings.BackgroundTransparency = (int)_transparency.Value;
        Settings.Theme = _theme.SelectedItem?.ToString() ?? "System";
        Settings.StartWithWindows = _startWindows.Checked;
        Settings.StartMinimized = _startMinimized.Checked;
        Settings.HideWhenDisconnected = _hideDisconnected.Checked;
        Settings.HideWhenZero = _hideZero.Checked;
        Settings.EnableUsageTracking = _tracking.Checked;
        Settings.ShowOverlay = _showOverlay.Checked;
        Settings.Normalize();
    }
}
