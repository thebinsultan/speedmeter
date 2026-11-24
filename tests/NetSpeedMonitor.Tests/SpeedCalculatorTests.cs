using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class SpeedCalculatorTests
{
    [Fact]
    public void FirstSample_ReturnsZero()
    {
        var calc = new SpeedCalculator();
        var (down, up) = calc.Update(1000, 500, DateTimeOffset.Now);
        Assert.Equal(0, down);
        Assert.Equal(0, up);
    }

    [Fact]
    public void ComputesBytesPerSecond()
    {
        var calc = new SpeedCalculator();
        var t0 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        calc.Update(0, 0, t0);
        var (down, up) = calc.Update(1024, 512, t0.AddSeconds(1));
        Assert.Equal(1024, down);
        Assert.Equal(512, up);
    }

    [Fact]
    public void CounterReset_ReturnsZeroNotNegative()
    {
        var calc = new SpeedCalculator();
        var t0 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        calc.Update(10_000, 10_000, t0);
        var (down, up) = calc.Update(100, 100, t0.AddSeconds(1));
        Assert.Equal(0, down);
        Assert.Equal(0, up);
    }

    [Fact]
    public void Delta_TracksBytesBetweenSamples()
    {
        var calc = new SpeedCalculator();
        var t0 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        calc.Update(100, 50, t0);
        var (d, u) = calc.Delta(300, 150);
        Assert.Equal(200, d);
        Assert.Equal(100, u);
    }
}
