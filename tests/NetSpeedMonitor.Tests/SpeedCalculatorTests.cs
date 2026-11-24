using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class SpeedCalculatorTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FirstSample_ReturnsZero()
    {
        var calc = new SpeedCalculator();
        var (down, up) = calc.Update(1000, 500, T0);
        Assert.Equal(0d, down);
        Assert.Equal(0d, up);
    }

    [Fact]
    public void ComputesBytesPerSecond()
    {
        var calc = new SpeedCalculator();
        calc.Update(0, 0, T0);
        var (down, up) = calc.Update(1024, 512, T0.AddSeconds(1));
        Assert.Equal(1024d, down, 3);
        Assert.Equal(512d, up, 3);
    }

    [Fact]
    public void UsesElapsedTimeNotAssumedInterval()
    {
        var calc = new SpeedCalculator();
        calc.Update(0, 0, T0);
        var (down, _) = calc.Update(2000, 0, T0.AddSeconds(2));
        Assert.Equal(1000d, down, 3);
    }

    [Fact]
    public void CounterReset_ReturnsZeroNotNegative()
    {
        var calc = new SpeedCalculator();
        calc.Update(10_000, 10_000, T0);
        var (down, up) = calc.Update(100, 100, T0.AddSeconds(1));
        Assert.Equal(0d, down);
        Assert.Equal(0d, up);
    }

    [Fact]
    public void ZeroElapsed_ReturnsZero()
    {
        var calc = new SpeedCalculator();
        calc.Update(10, 10, T0);
        var (down, up) = calc.Update(9999, 9999, T0);
        Assert.Equal(0d, down);
        Assert.Equal(0d, up);
    }

    [Fact]
    public void Delta_TracksBytesBetweenSamples()
    {
        var calc = new SpeedCalculator();
        calc.Update(100, 50, T0);
        var (d, u) = calc.Delta(300, 150);
        Assert.Equal(200L, d);
        Assert.Equal(100L, u);
    }

    [Fact]
    public void Delta_ClampsCounterReset()
    {
        var calc = new SpeedCalculator();
        calc.Update(500, 500, T0);
        var (d, u) = calc.Delta(10, 20);
        Assert.Equal(0L, d);
        Assert.Equal(0L, u);
    }

    [Fact]
    public void Reset_ForcesZeroOnNextSample()
    {
        var calc = new SpeedCalculator();
        calc.Update(0, 0, T0);
        calc.Update(5000, 5000, T0.AddSeconds(1));
        calc.Reset();
        var (down, up) = calc.Update(10_000, 10_000, T0.AddSeconds(2));
        Assert.Equal(0d, down);
        Assert.Equal(0d, up);
    }
}
