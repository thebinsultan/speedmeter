using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

public sealed class SpeedSample
{
    public double DownloadBytesPerSec { get; set; }
    public double UploadBytesPerSec { get; set; }
    public long DownloadDelta { get; set; }
    public long UploadDelta { get; set; }
    public NetworkAdapterInfo? Adapter { get; set; }
    public bool IsConnected => Adapter is not null;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    public static SpeedSample Empty => new();
}

/// <summary>
/// Polls real interface counters via System.Net.NetworkInformation (no speed-test traffic).
/// Counters are cumulative bytes; SpeedCalculator turns them into bytes/second.
/// </summary>
public sealed class NetworkMonitor
{
    // If more than this elapses between polls (sleep/hibernate, adapter outage) the
    // gap average would be meaningless, so the calculator is re-based instead.
    private static readonly TimeSpan StaleGap = TimeSpan.FromSeconds(10);

    private readonly Dictionary<string, SpeedCalculator> _calculators = new();
    private DateTimeOffset _lastPoll;
    private NetworkAdapterInfo? _current;

    public IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        var list = new List<NetworkAdapterInfo>();
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    var stats = nic.GetIPStatistics();
                    string? ipv4 = null;
                    try
                    {
                        foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipv4 = addr.Address.ToString();
                                break;
                            }
                        }
                    }
                    catch { /* adapter may be down */ }

                    long speed = 0;
                    try { speed = nic.Speed; } catch { /* not reported */ }

                    list.Add(new NetworkAdapterInfo
                    {
                        Id = nic.Id,
                        Name = nic.Name,
                        Description = nic.Description,
                        Type = MapType(nic.NetworkInterfaceType),
                        IsUp = nic.OperationalStatus == OperationalStatus.Up,
                        IPv4Address = ipv4,
                        BytesReceived = stats.BytesReceived,
                        BytesSent = stats.BytesSent,
                        LinkSpeedBps = speed
                    });
                }
                catch { /* skip faulty adapter */ }
            }
        }
        catch { /* no adapters available */ }
        return list;
    }

    public SpeedSample Poll(AppSettings settings)
    {
        var now = DateTimeOffset.Now;
        var adapters = GetAdapters();

        NetworkAdapterInfo? selected = null;
        if (!settings.AutoSelectAdapter)
            selected = AdapterSelector.FindById(adapters, settings.SelectedAdapterId);
        selected ??= AdapterSelector.SelectBest(adapters);
        _current = selected;

        if (selected is null)
        {
            _lastPoll = now;
            return SpeedSample.Empty;
        }

        if (!_calculators.TryGetValue(selected.Id, out var calc))
        {
            calc = new SpeedCalculator();
            _calculators[selected.Id] = calc;
        }

        bool stale = _lastPoll != default && now - _lastPoll > StaleGap;
        if (stale) calc.Reset();

        var (downDelta, upDelta) = calc.Delta(selected.BytesReceived, selected.BytesSent);
        var (down, up) = calc.Update(selected.BytesReceived, selected.BytesSent, now);
        _lastPoll = now;

        return new SpeedSample
        {
            DownloadBytesPerSec = down,
            UploadBytesPerSec = up,
            DownloadDelta = stale ? 0 : downDelta,
            UploadDelta = stale ? 0 : upDelta,
            Adapter = selected,
            Timestamp = now
        };
    }

    public NetworkAdapterInfo? Current => _current;

    private static string MapType(NetworkInterfaceType type) => type switch
    {
        NetworkInterfaceType.Ethernet or NetworkInterfaceType.GigabitEthernet
            or NetworkInterfaceType.FastEthernetT or NetworkInterfaceType.FastEthernetFx
            or NetworkInterfaceType.Ethernet3Megabit => "Ethernet",
        NetworkInterfaceType.Wireless80211 => "Wireless",
        NetworkInterfaceType.Loopback => "Loopback",
        NetworkInterfaceType.Tunnel => "Tunnel",
        _ => "Other"
    };
}
