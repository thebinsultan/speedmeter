using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class UnitFormatterTests
{
    [Theory]
    [InlineData(0d, "0.00 B/s")]
    [InlineData(512d, "512 B/s")]
    [InlineData(1024d, "1.00 KB/s")]
    [InlineData(131072d, "128 KB/s")]
    [InlineData(2569011.2, "2.45 MB/s")]
    [InlineData(1073741824d, "1.00 GB/s")]
    [InlineData(1099511627776d, "1.00 TB/s")]
    public void FormatSpeed_Bytes(double bps, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatSpeed(bps, useBits: false));
    }

    [Theory]
    [InlineData(1024d, "8.00 Kbps")]
    [InlineData(1048576d, "8.00 Mbps")]
    [InlineData(131072d, "1.00 Mbps")]
    public void FormatSpeed_Bits(double bps, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatSpeed(bps, useBits: true));
    }

    [Theory]
    [InlineData(0L, "0.00 B")]
    [InlineData(1024L, "1.00 KB")]
    [InlineData(5242880L, "5.00 MB")]
    [InlineData(1073741824L, "1.00 GB")]
    public void FormatBytes_Works(long bytes, string expected)
    {
        Assert.Equal(expected, UnitFormatter.FormatBytes(bytes));
    }

    [Fact]
    public void NegativeSpeed_ClampedToZero()
    {
        Assert.Equal("0.00 B/s", UnitFormatter.FormatSpeed(-5));
    }

    [Fact]
    public void NegativeBytes_ClampedToZero()
    {
        Assert.Equal("0.00 B", UnitFormatter.FormatBytes(-1));
    }
}
