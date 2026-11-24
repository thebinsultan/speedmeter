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
    public void ReturnsNullWhenNoneUp()
    {
        var list = new List<NetworkAdapterInfo> { Adapter("a", "Ethernet", up: false) };
        Assert.Null(AdapterSelector.SelectBest(list));
    }

    [Fact]
    public void FindById_Works()
    {
        var list = new List<NetworkAdapterInfo> { Adapter("x", "Ethernet") };
        Assert.Equal("x", AdapterSelector.FindById(list, "x")?.Id);
        Assert.Null(AdapterSelector.FindById(list, "missing"));
    }
}
