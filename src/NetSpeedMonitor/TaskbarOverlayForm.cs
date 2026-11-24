using System.Runtime.InteropServices;
using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

/// <summary>
/// Borderless, taskbar-aligned overlay. Windows 11's taskbar is XAML-hosted and
/// cannot be injected into, so this overlay visually matches the taskbar instead.
/// It is a normal top-level window (WS_EX_NOACTIVATE | TOOLWINDOW) — no code is
/// injected into Explorer and no system files are touched.
///
/// The window is sized once from placeholder strings ("888.8 MB/s") so changing
/// numbers never resize or jitter the overlay.
/// </summary>
public sealed class TaskbarOverlayForm : Form
{
    private const int WM_DISPLAYCHANGE = 0x007E;
    private const int WM_DPICHANGED = 0x02E0;
    private const int WM_SETTINGCHANGE = 0x001A;

    private const string DownTemplate = "\u2193 888.8 MB/s";
    private const string UpTemplate = "\u2191 888.8 MB/s";

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RegisterWindowMessage(string lpString);

    private string _downloadText = "\u2193 0.00 B/s";
    private string _uploadText = "\u2191 0.00 B/s";
    private AppSettings _settings;
    private readonly int _taskbarCreatedMessage;
    private readonly Font _defaultFont;
    private Font _customFont;
    private bool _dragging;
    private bool _dragMoved;
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
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
        _defaultFont = Font;
        _customFont = Font;

        ApplySettings(settings);

        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left && !_dragMoved) OverlayClicked?.Invoke();
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
        if (m.Msg == _taskbarCreatedMessage || m.Msg == WM_DISPLAYCHANGE ||
            m.Msg == WM_DPICHANGED || m.Msg == WM_SETTINGCHANGE)
        {
            if (IsHandleCreated)
            {
                try { BeginInvoke(new Action(RefreshLayout)); } catch { /* shutting down */ }
            }
        }
        base.WndProc(ref m);
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        BackColor = ThemeHelper.Background(settings.Theme);
        Opacity = 1.0 - (settings.BackgroundTransparency / 100.0);

        Font resolved;
        try { resolved = new Font(settings.FontFamily, settings.FontSize, FontStyle.Regular, GraphicsUnit.Point); }
        catch { resolved = (Font)_defaultFont.Clone(); }

        var previous = _customFont;
        _customFont = resolved;
        Font = resolved;
        if (!ReferenceEquals(previous, _defaultFont) && !ReferenceEquals(previous, resolved))
            previous.Dispose();

        RefreshLayout();
    }

    public void SetSpeeds(double downloadBps, double uploadBps)
    {
        string down = "\u2193 " + UnitFormatter.FormatSpeed(downloadBps, _settings.UseBitsPerSecond);
        string up = "\u2191 " + UnitFormatter.FormatSpeed(uploadBps, _settings.UseBitsPerSecond);
        if (down == _downloadText && up == _uploadText) return;
        _downloadText = down;
        _uploadText = up;
        Invalidate();
    }

    public void RefreshLayout()
    {
        FitWithTemplates();
        Reposition();
        Invalidate();
    }

    private void FitWithTemplates()
    {
        try
        {
            var downSize = TextRenderer.MeasureText(DownTemplate, Font);
            var upSize = TextRenderer.MeasureText(UpTemplate, Font);
            Size size = _settings.LayoutVertical
                ? new Size(Math.Max(downSize.Width, upSize.Width) + 8, downSize.Height + upSize.Height - 2)
                : new Size(downSize.Width + upSize.Width + 12, Math.Max(downSize.Height, upSize.Height));

            if (size.Width < 24) size.Width = 24;
            if (size.Height < 16) size.Height = 16;
            if (Size != size) Size = size;
        }
        catch { /* keep current size */ }
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
        const TextFormatFlags Flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        if (_settings.LayoutVertical)
        {
            int lineHeight = Font.Height;
            var downRect = new Rectangle(4, 0, Width - 8, lineHeight + 2);
            var upRect = new Rectangle(4, lineHeight - 1, Width - 8, lineHeight + 2);
            TextRenderer.DrawText(g, _downloadText, Font, downRect, downColor, Flags);
            TextRenderer.DrawText(g, _uploadText, Font, upRect, upColor, Flags);
        }
        else
        {
            var downRect = new Rectangle(4, 0, Width - 8, Height);
            TextRenderer.DrawText(g, _downloadText, Font, downRect, downColor, Flags);
            int w = TextRenderer.MeasureText(g, _downloadText, Font).Width;
            var upRect = new Rectangle(4 + w + 8, 0, Width - 8, Height);
            TextRenderer.DrawText(g, _uploadText, Font, upRect, upColor, Flags);
        }
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _dragging = true;
        _dragMoved = false;
        _dragStart = Cursor.Position;
        _formStart = Location;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        int dx = Cursor.Position.X - _dragStart.X;
        int dy = Cursor.Position.Y - _dragStart.Y;
        if (Math.Abs(dx) + Math.Abs(dy) <= 4) return;
        _dragMoved = true;
        Location = new Point(_formStart.X + dx, _formStart.Y + dy);
    }

    /// <summary>Position is reported once, on release, so the offset is saved once per drag.</summary>
    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        if (_dragMoved) OverlayMoved?.Invoke(Location.X, Location.Y);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && !ReferenceEquals(_customFont, _defaultFont)) _customFont.Dispose();
    }
}
