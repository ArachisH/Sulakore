using Sulakore.Habbo;
using Sulakore.Network;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HotelEndPointTests
{
    [Fact]
    public void HotelEndPoint_ShouldCreateForValidHostname()
    {
        const string HOSTNAME = "game-us.habbo.com";

        var endpoint = HotelEndPoint.Create(HOSTNAME);

        Assert.Equal(HOSTNAME, endpoint.Host);
        Assert.Equal(HHotel.US, endpoint.Hotel);
    }

    [Fact]
    public async Task HotelEndPoint_ShouldParseValidHostnameAsync()
    {
        const string HOSTNAME = "game-us.habbo.com";
        const int PORT = 30001;

        var endpoint = await HotelEndPoint.ParseAsync(HOSTNAME, PORT);

        Assert.Equal(PORT, endpoint.Port);
        Assert.Equal(HOSTNAME, endpoint.Host);
        Assert.Equal(HHotel.US, endpoint.Hotel);
    }
}
