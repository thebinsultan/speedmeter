using System.Globalization;

namespace NetSpeedMonitor.Core;

/// <summary>Formats speeds and byte totals with automatic unit selection.</summary>
public static class UnitFormatter
{
    private static readonly string[] ByteUnits = { "B/s", "KB/s", "MB/s", "GB/s", "TB/s" };
    private static readonly string[] BitUnits = { "bps", "Kbps", "Mbps", "Gbps", "Tbps" };
    private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB", "PB" };

    public static string FormatSpeed(double bytesPerSecond, bool useBits = false)
    {
        if (bytesPerSecond < 0 || double.IsNaN(bytesPerSecond) || double.IsInfinity(bytesPerSecond))
            bytesPerSecond = 0;

        double value = useBits ? bytesPerSecond * 8.0 : bytesPerSecond;
        string[] units = useBits ? BitUnits : ByteUnits;
        int i = 0;
        while (value >= 1024.0 && i < units.Length - 1)
        {
            value /= 1024.0;
            i++;
        }
        return $"{FormatNumber(value)} {units[i]}";
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 0) bytes = 0;
        double value = bytes;
        int i = 0;
        while (value >= 1024.0 && i < SizeUnits.Length - 1)
        {
            value /= 1024.0;
            i++;
        }
        return $"{FormatNumber(value)} {SizeUnits[i]}";
    }

    private static string FormatNumber(double value)
    {
        // 2.45 MB/s style: 2 decimals <10, 1 decimal <100, integer >=100.
        string fmt = value >= 100 ? "0" : value >= 10 ? "0.0" : "0.00";
        return value.ToString(fmt, CultureInfo.InvariantCulture);
    }
}
