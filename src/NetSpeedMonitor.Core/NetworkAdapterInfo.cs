namespace NetSpeedMonitor.Core;

/// <summary>Platform-independent snapshot of a network adapter.</summary>
public sealed class NetworkAdapterInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Ethernet | Wireless | Other | Loopback | Tunnel | Virtual</summary>
    public string Type { get; set; } = "Other";
    public bool IsUp { get; set; }
    public string? IPv4Address { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public long LinkSpeedBps { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Description : Name;
    public string ConnectionType => Type switch
    {
        "Ethernet" => "Ethernet",
        "Wireless" => "Wi-Fi",
        _ => Type
    };
}
