namespace NetSpeedMonitor.Core;

/// <summary>
/// Computes bytes/second from cumulative interface counters.
/// Handles counter resets (adapter restart, wrap) by returning 0 for that sample.
/// </summary>
public sealed class SpeedCalculator
{
    private long? _prevReceived;
    private long? _prevSent;
    private DateTimeOffset? _prevTime;

    public (double DownloadBytesPerSec, double UploadBytesPerSec) Update(
        long bytesReceived, long bytesSent, DateTimeOffset now)
    {
        if (_prevReceived is null || _prevSent is null || _prevTime is null)
        {
            _prevReceived = bytesReceived;
            _prevSent = bytesSent;
            _prevTime = now;
            return (0, 0);
        }

        double elapsed = (now - _prevTime.Value).TotalSeconds;
        if (elapsed <= 0)
            return (0, 0);

        long downDelta = bytesReceived - _prevReceived.Value;
        long upDelta = bytesSent - _prevSent.Value;

        // Counter reset / adapter restart: treat as 0 rather than negative.
        if (downDelta < 0) downDelta = 0;
        if (upDelta < 0) upDelta = 0;

        double down = downDelta / elapsed;
        double up = upDelta / elapsed;

        _prevReceived = bytesReceived;
        _prevSent = bytesSent;
        _prevTime = now;
        return (down, up);
    }

    public (long DownloadDelta, long UploadDelta) Delta(
        long bytesReceived, long bytesSent)
    {
        if (_prevReceived is null || _prevSent is null)
            return (0, 0);
        long d = bytesReceived - _prevReceived.Value;
        long u = bytesSent - _prevSent.Value;
        return (d < 0 ? 0 : d, u < 0 ? 0 : u);
    }

    public void Reset()
    {
        _prevReceived = null;
        _prevSent = null;
        _prevTime = null;
    }
}
