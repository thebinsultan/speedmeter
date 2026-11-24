using NetSpeedMonitor.Core;

namespace NetSpeedMonitor.Tests;

public class AdapterSelectorTests
{
    private static NetworkAdapterInfo Adapter(string id, string type, bool up = true, long bytes = 0) =>
        new() { Id = id, Name = id, Type = type, IsUp = up, BytesReceived = bytes };

    [Fact]
    public void PrefersEthernetOverWifi()
    {
        var list = new List<NetworkAdapterInfo> { Adapter("wifi", "Wireless"), Adapter("eth", "Ethernet") };
        Assert.Equal("eth", AdapterSelector.SelectBest(list)?.Id);
    }

    [Fact]
    public void IgnoresDownAndLoopback()
    {
        var list = new List<NetworkAdapterInfo>
        {
            Adapter("down", "Ethernet", up: false),
            Adapter("lo", "Loopback"),
            Adapter("wifi", "Wireless")
        };
        Assert.Equal("wifi", AdapterSelector.SelectBest(list)?.Id);
    }

    [Fact]
    public void SameType_PrefersMoreTraffic()
    {
        var list = new List<NetworkAdapterInfo>
        {
            Adapter("idle", "Ethernet", bytes: 10),
            Adapter("busy", "Ethernet", bytes: 10_000)
        };
        Assert.Equal("busy", AdapterSelector.SelectBest(list)?.Id);
    }

    [Fact]
    public void ReturnsNullWhenNoneUp()
    {
        var list = new List<NetworkAdapterInfo> { Adapter("a", "Ethernet", up: false) };
        Assert.Null(AdapterSelector.SelectBest(list));
    }

    [Fact]
    public void ReturnsNullForEmptyList()
    {
        Assert.Null(AdapterSelector.SelectBest(Array.Empty<NetworkAdapterInfo>()));
    }

    [Fact]
    public void FindById_IsCaseInsensitive_AndHandlesMissing()
    {
        var list = new List<NetworkAdapterInfo> { Adapter("ABC", "Ethernet") };
        Assert.Equal("ABC", AdapterSelector.FindById(list, "abc")?.Id);
        Assert.Null(AdapterSelector.FindById(list, "missing"));
        Assert.Null(AdapterSelector.FindById(list, null));
    }

    [Fact]
    public void DisplayName_FallsBackToDescription()
    {
        var adapter = new NetworkAdapterInfo { Name = "", Description = "Intel Ethernet" };
        Assert.Equal("Intel Ethernet", adapter.DisplayName);
    }
}
