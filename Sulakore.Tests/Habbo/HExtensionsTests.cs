using Sulakore.Habbo;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HExtensionsTests
{
    [Theory]
    [InlineData("br", HHotel.BR)]
    [InlineData("com", HHotel.US)]
    [InlineData("hhtr", HHotel.TR)]
    [InlineData("com.br", HHotel.BR)]
    [InlineData("habbo.com", HHotel.US)]
    [InlineData("habbo.com.tr", HHotel.TR)]
    [InlineData("game-fi.habbo.com", HHotel.FI)]
    [InlineData("game-us.habbo.com", HHotel.US)]
    [InlineData("hhfi-eff0eba1b6a6a9bc253ae9299c407757", HHotel.FI)]
    public void HExtensions_HHotel_ToHotel_ShouldParseValidValues(string value, HHotel expectedHotel)
    {
        Assert.Equal(expectedHotel, value.AsSpan().ToHotel());
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData(" ")]
    [InlineData("hh1")]
    [InlineData("hhuk")]
    [InlineData("hhs1")]
    [InlineData("0001")]
    [InlineData("huuhaakokeilu")]
    public void HExtensions_HHotel_ToHotel_ShouldReturnUnknownForInvalidValues(string value)
    {
        Assert.Equal(HHotel.Unknown, value.AsSpan().ToHotel());
    }
}
