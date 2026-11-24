namespace NetSpeedMonitor;

/// <summary>Simple real-time download/upload line graph (GDI+, no dependencies).</summary>
public sealed class SpeedGraphControl : Control
{
    private readonly Queue<(double Down, double Up)> _history = new();
    private const int MaxPoints = 60;

    public SpeedGraphControl()
    {
        DoubleBuffered = true;
        MinimumSize = new Size(320, 120);
    }

    public void AddSample(double down, double up)
    {
        _history.Enqueue((down, up));
        while (_history.Count > MaxPoints) _history.Dequeue();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(0x1E, 0x1E, 0x1E));
        if (_history.Count < 2) return;
        double max = Math.Max(1, _history.Max(p => Math.Max(p.Down, p.Up)));
        DrawLine(g, max, p => p.Down, Color.LimeGreen);
        DrawLine(g, max, p => p.Up, Color.DodgerBlue);
    }

    private void DrawLine(Graphics g, double max, Func<(double Down, double Up), double> pick, Color color)
    {
        var pts = _history.Select((p, i) =>
            new PointF(i * (Width - 1f) / (MaxPoints - 1), Height - 2 - (float)(pick(p) / max * (Height - 6))))
            .ToArray();
        using var pen = new Pen(color, 1.5f);
        if (pts.Length > 1) g.DrawLines(pen, pts);
    }
}
