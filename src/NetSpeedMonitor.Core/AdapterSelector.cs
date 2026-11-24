namespace NetSpeedMonitor.Core;

/// <summary>Automatic adapter selection: Ethernet preferred, then Wi-Fi, then others.</summary>
public static class AdapterSelector
{
    public static NetworkAdapterInfo? SelectBest(IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        var candidates = adapters
            .Where(a => a.IsUp && a.Type is not ("Loopback" or "Tunnel"))
            .ToList();
        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        return candidates
            .OrderByDescending(a => Score(a))
            .ThenByDescending(a => a.BytesReceived + a.BytesSent)
            .First();
    }

    public static NetworkAdapterInfo? FindById(IReadOnlyList<NetworkAdapterInfo> adapters, string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return adapters.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private static int Score(NetworkAdapterInfo a) => a.Type switch
    {
        "Ethernet" => 30,
        "Wireless" => 20,
        "Other" => 10,
        _ => 0
    };
}
