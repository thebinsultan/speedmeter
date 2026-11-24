using System.Runtime.InteropServices;
using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

/// <summary>
/// Borderless, taskbar-aligned overlay. Windows 11's taskbar is XAML-hosted and
/// cannot be injected into, so this reliable overlay visually matches the taskbar.
/// </summary>
public sealed class TaskbarOverlayForm : Form
{
    private const int WM_DISPLAYCHANGE = 0x007E;
    private const int WM_DPICHANGED = 0x02E0;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RegisterWindowMessage(string lpString);

    private string _downloadText = "↓ 0 B/s";
    private string _uploadText = "↑ 0 B/s";
    private AppSettings _settings;
    private int _taskbarCreatedMessage;
    private bool _dragging;
    private Point _dragStart;
    private Point _formStart;

    public event Action? OverlayClicked;
    public event Action<int, int>? OverlayMoved;

    public TaskbarOverlayForm(AppSettings settings)
    {
        _settings = settings;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        ShowIcon = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        DoubleBuffered = true;
        _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
        ApplySettings(settings);
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += (_, _) => _dragging = false;
        MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left && !_dragging)
                OverlayClicked?.Invoke();
        };
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var p = base.CreateParams;
            // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST
            p.ExStyle |= 0x08000000 | 0x00000080 | 0x00040000;
            return p;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == _taskbarCreatedMessage || m.Msg == WM_DISPLAYCHANGE || m.Msg == WM_DPICHANGED)
        {
            BeginInvoke(new Action(Reposition));
        }
        base.WndProc(ref m);
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        bool dark = ThemeHelper.IsDarkMode(settings.Theme);
        BackColor = ThemeHelper.Background(settings.Theme);
        Opacity = 1.0 - (settings.BackgroundTransparency / 100.0);
        try
        {
            Font = new Font(settings.FontFamily, settings.FontSize, FontStyle.Regular);
        }
        catch { Font = new Font("Segoe UI", 9f); }
        // Size to content: two lines or one line.
        try
        {
            using var g = CreateGraphics();
            var downSize = g.MeasureString(_downloadText, Font);
            var upSize = g.MeasureString(_uploadText, Font);
            if (settings.LayoutVertical)
                Size = new Size((int)Math.Ceiling(Math.Max(downSize.Width, upSize.Width)) + 8,
                                (int)Math.Ceiling(downSize.Height + upSize.Height) + 2);
            else
                Size = new Size((int)Math.Ceiling(downSize.Width + upSize.Width) + 12,
                                (int)Math.Ceiling(Math.Max(downSize.Height, upSize.Height)) + 2);
        }
        catch { Size = settings.LayoutVertical ? new Size(110, 32) : new Size(200, 18); }
        Reposition();
        Invalidate();
    }

    public void SetSpeeds(double downloadBps, double uploadBps)
    {
        _downloadText = "↓ " + UnitFormatter.FormatSpeed(downloadBps, _settings.UseBitsPerSecond);
        _uploadText = "↑ " + UnitFormatter.FormatSpeed(uploadBps, _settings.UseBitsPerSecond);
        Invalidate();
    }

    public void Reposition()
    {
        try { Location = TaskbarHelper.ComputeOverlayPosition(Size, _settings.OffsetX, _settings.OffsetY); }
        catch { /* keep current position */ }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(BackColor);
        var downColor = ThemeHelper.ParseColor(_settings.DownloadColor, Color.LimeGreen);
        var upColor = ThemeHelper.ParseColor(_settings.UploadColor, Color.DodgerBlue);
        using var downBrush = new SolidBrush(downColor);
        using var upBrush = new SolidBrush(upColor);
        using var fgBrush = new SolidBrush(ThemeHelper.Foreground(_settings.Theme));
        if (_settings.LayoutVertical)
        {
            g.DrawString(_downloadText, Font, downBrush, 4, 0);
            g.DrawString(_uploadText, Font, upBrush, 4, Font.GetHeight(g) - 1);
        }
        else
        {
            g.DrawString(_downloadText, Font, downBrush, 4, 2);
            var w = g.MeasureString(_downloadText, Font).Width;
            g.DrawString(_uploadText, Font, upBrush, 4 + w + 8, 2);
        }
        _ = fgBrush;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _dragging = true;
        _dragStart = Cursor.Position;
        _formStart = Location;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        var dx = Cursor.Position.X - _dragStart.X;
        var dy = Cursor.Position.Y - _dragStart.Y;
        if (Math.Abs(dx) + Math.Abs(dy) > 4)
        {
            Location = new Point(_formStart.X + dx, _formStart.Y + dy);
            OverlayMoved?.Invoke(Location.X, Location.Y);
        }
    }
}
