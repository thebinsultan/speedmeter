namespace NetSpeedMonitor;

/// <summary>Simple real-time download/upload line graph (GDI+, no dependencies).</summary>
public sealed class SpeedGraphControl : Control
{
    private const int MaxPoints = 60;
    private readonly Queue<(double Down, double Up)> _history = new();
    private bool _dark = true;

    public SpeedGraphControl()
    {
        DoubleBuffered = true;
        MinimumSize = new Size(200, 100);
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    public void SetTheme(bool dark)
    {
        _dark = dark;
        Invalidate();
    }

    public void AddSample(double down, double up)
    {
        if (double.IsNaN(down) || double.IsInfinity(down)) down = 0;
        if (double.IsNaN(up) || double.IsInfinity(up)) up = 0;
        _history.Enqueue((down, up));
        while (_history.Count > MaxPoints) _history.Dequeue();
        if (Visible) Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(_dark ? Color.FromArgb(0x1E, 0x1E, 0x1E) : Color.FromArgb(0xFA, 0xFA, 0xFA));
        if (Width < 8 || Height < 8) return;

        if (_history.Count < 2)
        {
            using var hint = new SolidBrush(_dark ? Color.Gray : Color.DimGray);
            g.DrawString("Collecting data…", Font, hint, 8, 8);
            return;
        }

        double max = 1;
        foreach (var p in _history) max = Math.Max(max, Math.Max(p.Down, p.Up));

        DrawLine(g, max, p => p.Down, Color.FromArgb(0x2E, 0xCC, 0x71));
        DrawLine(g, max, p => p.Up, Color.FromArgb(0x34, 0x98, 0xDB));
    }

    private void DrawLine(Graphics g, double max, Func<(double Down, double Up), double> pick, Color color)
    {
        var points = _history.ToArray();
        var pts = new PointF[points.Length];
        float step = points.Length > 1 ? (Width - 8f) / (MaxPoints - 1) : 0f;
        for (int i = 0; i < points.Length; i++)
        {
            float x = 4 + i * step;
            float y = Height - 4 - (float)(Math.Clamp(pick(points[i]) / max, 0, 1) * (Height - 10));
            pts[i] = new PointF(x, y);
        }
        using var pen = new Pen(color, 1.5f);
        if (pts.Length > 1) g.DrawLines(pen, pts);
    }
}
