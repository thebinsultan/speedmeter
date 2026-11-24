using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class UnitFormatterTests
{
    [Theory]
    [InlineData(0, "0.00 B/s")]
    [InlineData(512, "512 B/s")]
    [InlineData(1024, "1.00 KB/s")]
    [InlineData(128 * 1024, "128 KB/s")]
    [InlineData(2569011.2, "2.45 MB/s")]
    [InlineData(1024.0 * 1024 * 1024, "1.00 GB/s")]
    public void FormatSpeed_Bytes(double bps, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatSpeed(bps, useBits: false));
    }

    [Theory]
    [InlineData(1024, "8.00 Kbps")]
    [InlineData(1024 * 1024, "8.00 Mbps")]
    public void FormatSpeed_Bits(double bps, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatSpeed(bps, useBits: true));
    }

    [Theory]
    [InlineData(0, "0.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(5 * 1024 * 1024, "5.00 MB")]
    public void FormatBytes_Works(long bytes, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatBytes(bytes));
    }

    [Fact]
    public void FormatSpeed_Negative_ClampedToZero()
    {
        Assert.Equal("0.00 B/s", UnitFormatter.FormatSpeed(-5));
    }
}
