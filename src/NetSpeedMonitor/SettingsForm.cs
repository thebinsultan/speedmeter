using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

/// <summary>Compact settings dialog. Edits a copy of the settings and persists on Save.</summary>
public sealed class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly IReadOnlyList<NetworkAdapterInfo> _adapters;

    private readonly ComboBox _interval = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly CheckBox _bits = new() { Text = "Show speeds in bits/s (Mbps)", AutoSize = true };
    private readonly CheckBox _autoAdapter = new() { Text = "Auto-select adapter", AutoSize = true };
    private readonly ComboBox _adapter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly CheckBox _vertical = new() { Text = "Two-line (vertical) layout", AutoSize = true };
    private readonly ComboBox _fontFamily = new() { DropDownStyle = ComboBoxStyle.DropDown, Width = 180 };
    private readonly NumericUpDown _fontSize = new() { Minimum = 6, Maximum = 24, DecimalPlaces = 1, Increment = 0.5M, Width = 80 };
    private readonly TextBox _downColor = new() { Width = 100 };
    private readonly TextBox _upColor = new() { Width = 100 };
    private readonly NumericUpDown _offsetX = new() { Minimum = -1000, Maximum = 1000, Width = 80 };
    private readonly NumericUpDown _offsetY = new() { Minimum = -1000, Maximum = 1000, Width = 80 };
    private readonly NumericUpDown _transparency = new() { Minimum = 0, Maximum = 90, Width = 80 };
    private readonly ComboBox _theme = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly CheckBox _startWithWindows = new() { Text = "Start with Windows", AutoSize = true };
    private readonly CheckBox _startMinimized = new() { Text = "Start minimized", AutoSize = true };
    private readonly CheckBox _hideDisconnected = new() { Text = "Hide when disconnected", AutoSize = true };
    private readonly CheckBox _hideZero = new() { Text = "Hide when speed is zero", AutoSize = true };
    private readonly CheckBox _tracking = new() { Text = "Track data usage history", AutoSize = true };
    private readonly CheckBox _showOverlay = new() { Text = "Show speeds on taskbar", AutoSize = true };

    public event Action? Saved;

    public SettingsForm(AppSettings settings, IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        _settings = settings;
        _adapters = adapters;

        Text = "NetSpeed Monitor Settings";
        ClientSize = new Size(430, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _interval.Items.AddRange(new object[] { "500 ms", "1 s", "2 s", "5 s" });
        _theme.Items.AddRange(new object[] { "System", "Light", "Dark" });

        var families = new List<string> { "Segoe UI", "Segoe UI Variable Text", "Tahoma", "Consolas", "Arial", "Verdana" };
        try
        {
            foreach (var f in System.Drawing.FontFamily.Families)
                if (!families.Contains(f.Name, StringComparer.OrdinalIgnoreCase)) families.Add(f.Name);
        }
        catch { /* font enumeration unavailable */ }
        _fontFamily.Items.AddRange(families.Cast<object>().ToArray());

        foreach (var a in _adapters) _adapter.Items.Add($"{a.DisplayName} ({a.ConnectionType})");

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16, 12, 16, 12),
            AutoScroll = true
        };

        panel.Controls.Add(Header("Display"));
        panel.Controls.Add(Labeled("Refresh interval", _interval));
        panel.Controls.Add(_bits);
        panel.Controls.Add(Labeled("Adapter", _autoAdapter));
        panel.Controls.Add(_adapter);
        panel.Controls.Add(_vertical);
        panel.Controls.Add(Labeled("Taskbar visibility", _showOverlay));
        panel.Controls.Add(Labeled("Theme", _theme));
        panel.Controls.Add(Labeled("Background transparency (%)", _transparency));

        panel.Controls.Add(Header("Appearance"));
        panel.Controls.Add(Labeled("Font", _fontFamily));
        panel.Controls.Add(Labeled("Font size", _fontSize));
        panel.Controls.Add(Labeled("Download color", _downColor));
        panel.Controls.Add(Labeled("Upload color", _upColor));
        panel.Controls.Add(Labeled("Offset X", _offsetX));
        panel.Controls.Add(Labeled("Offset Y", _offsetY));

        panel.Controls.Add(Header("Behaviour"));
        panel.Controls.Add(_startWithWindows);
        panel.Controls.Add(_startMinimized);
        panel.Controls.Add(_hideDisconnected);
        panel.Controls.Add(_hideZero);
        panel.Controls.Add(_tracking);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 48,
            Padding = new Padding(8)
        };
        var save = new Button { Text = "Save", Width = 88, Height = 30, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", Width = 88, Height = 30, DialogResult = DialogResult.Cancel };
        save.Click += (_, _) => Save();
        cancel.Click += (_, _) => Close();
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(save);

        Controls.Add(panel);
        Controls.Add(buttons);
        AcceptButton = save;
        CancelButton = cancel;

        _autoAdapter.CheckedChanged += (_, _) => _adapter.Enabled = !_autoAdapter.Checked;
        LoadValues();
        ApplyTheme();
    }

    private static Label Header(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        Margin = new Padding(0, 8, 0, 2)
    };

    private static Control Labeled(string text, Control control)
    {
        var host = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 2)
        };
        host.Controls.Add(new Label
        {
            Text = text,
            AutoSize = false,
            Width = 150,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 3, 8, 0)
        });
        control.Margin = new Padding(0, 2, 0, 2);
        host.Controls.Add(control);
        return host;
    }

    private void LoadValues()
    {
        _interval.SelectedIndex = _settings.RefreshIntervalMs switch
        {
            500 => 0,
            2000 => 2,
            5000 => 3,
            _ => 1
        };
        _bits.Checked = _settings.UseBitsPerSecond;
        _autoAdapter.Checked = _settings.AutoSelectAdapter;
        _adapter.Enabled = !_settings.AutoSelectAdapter;
        int idx = _adapters.ToList().FindIndex(a => a.Id == _settings.SelectedAdapterId);
        _adapter.SelectedIndex = idx >= 0 ? idx : (_adapters.Count > 0 ? 0 : -1);
        _vertical.Checked = _settings.LayoutVertical;
        _fontFamily.Text = _settings.FontFamily;
        _fontSize.Value = (decimal)Math.Clamp(_settings.FontSize, 6f, 24f);
        _downColor.Text = _settings.DownloadColor;
        _upColor.Text = _settings.UploadColor;
        _offsetX.Value = Math.Clamp(_settings.OffsetX, -1000, 1000);
        _offsetY.Value = Math.Clamp(_settings.OffsetY, -1000, 1000);
        _transparency.Value = Math.Clamp(_settings.BackgroundTransparency, 0, 90);
        _theme.SelectedItem = _settings.Theme;
        if (_theme.SelectedIndex < 0) _theme.SelectedIndex = 0;
        _startWithWindows.Checked = _settings.StartWithWindows;
        _startMinimized.Checked = _settings.StartMinimized;
        _hideDisconnected.Checked = _settings.HideWhenDisconnected;
        _hideZero.Checked = _settings.HideWhenZero;
        _tracking.Checked = _settings.EnableUsageTracking;
        _showOverlay.Checked = _settings.ShowOverlay;
    }

    private void Save()
    {
        _settings.RefreshIntervalMs = _interval.SelectedIndex switch
        {
            0 => 500,
            2 => 2000,
            3 => 5000,
            _ => 1000
        };
        _settings.UseBitsPerSecond = _bits.Checked;
        _settings.AutoSelectAdapter = _autoAdapter.Checked;
        if (!_autoAdapter.Checked && _adapter.SelectedIndex >= 0 && _adapter.SelectedIndex < _adapters.Count)
            _settings.SelectedAdapterId = _adapters[_adapter.SelectedIndex].Id;
        _settings.LayoutVertical = _vertical.Checked;
        _settings.FontFamily = string.IsNullOrWhiteSpace(_fontFamily.Text) ? "Segoe UI" : _fontFamily.Text.Trim();
        _settings.FontSize = (float)_fontSize.Value;
        _settings.DownloadColor = _downColor.Text.Trim();
        _settings.UploadColor = _upColor.Text.Trim();
        _settings.OffsetX = (int)_offsetX.Value;
        _settings.OffsetY = (int)_offsetY.Value;
        _settings.BackgroundTransparency = (int)_transparency.Value;
        _settings.Theme = _theme.SelectedItem?.ToString() ?? "System";
        _settings.StartWithWindows = _startWithWindows.Checked;
        _settings.StartMinimized = _startMinimized.Checked;
        _settings.HideWhenDisconnected = _hideDisconnected.Checked;
        _settings.HideWhenZero = _hideZero.Checked;
        _settings.EnableUsageTracking = _tracking.Checked;
        _settings.ShowOverlay = _showOverlay.Checked;
        _settings.Normalize();

        try { SettingsStore.SaveSettings(_settings); }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not save settings: " + ex.Message, "NetSpeed Monitor",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        Saved?.Invoke();
    }

    private void ApplyTheme()
    {
        bool dark = ThemeHelper.IsDarkMode(_settings.Theme);
        var back = ThemeHelper.Background(_settings.Theme);
        var fore = ThemeHelper.Foreground(_settings.Theme);
        BackColor = back;
        ForeColor = fore;
        foreach (Control top in Controls) PaintRecursive(top, back, fore, dark);
    }

    private static void PaintRecursive(Control control, Color back, Color fore, bool dark)
    {
        control.BackColor = back;
        control.ForeColor = fore;
        if (control is TextBox or ComboBox or NumericUpDown or Button)
        {
            control.BackColor = dark ? Color.FromArgb(0x2B, 0x2B, 0x2B) : Color.White;
            control.ForeColor = fore;
        }
        foreach (Control child in control.Controls) PaintRecursive(child, back, fore, dark);
    }
}
