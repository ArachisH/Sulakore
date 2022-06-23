using Sulakore.Habbo.Badge;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HBadgeTests
{
    [Fact]
    public void HBadge_ToString_SingleLigthGrayTriangleBase()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Triangle, HBadgeColor.LightGray)
        };

        Assert.Equal("b03120", badge.ToString());
    }

    [Fact]
    public void HBadge_ToString_PurpleSquareBaseWithDarkRedSkullSymbol()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Square, HBadgeColor.Purple),
            new HBadgeSymbolPart(HBadgeSymbol.Bobba, HBadgeColor.DarkRed, HBadgePosition.Center)
        };

        Assert.Equal("b01200s06104", badge.ToString());
    }

    [Fact]
    public void HBadge_ToString_LightGreenTriangleOnTopOfSquare()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Square, HBadgeColor.LightGreen),
            new HBadgeBasePart(HBadgeBase.Triangle, HBadgeColor.LightGreen)
        };

        Assert.Equal("b01030b03030", badge.ToString());
    }

    [Fact]
    public void HBadge_ToString_NumberLetterSymbols()
    {
        var badge = new HBadge
        {
            new HBadgeSymbolPart(HBadgeSymbol.One, HBadgeColor.White, HBadgePosition.TopLeft),
            new HBadgeSymbolPart(HBadgeSymbol.Two, HBadgeColor.White, HBadgePosition.TopCenter),
            new HBadgeSymbolPart(HBadgeSymbol.Three, HBadgeColor.White, HBadgePosition.TopRight),
            new HBadgeSymbolPart(HBadgeSymbol.Four, HBadgeColor.White, HBadgePosition.MiddleLeft),
            new HBadgeSymbolPart(HBadgeSymbol.Five, HBadgeColor.White, HBadgePosition.Center),
            new HBadgeSymbolPart(HBadgeSymbol.Six, HBadgeColor.White, HBadgePosition.MiddleRight),
            new HBadgeSymbolPart(HBadgeSymbol.Seven, HBadgeColor.White, HBadgePosition.BottomLeft),
            new HBadgeSymbolPart(HBadgeSymbol.Eight, HBadgeColor.White, HBadgePosition.BottomCenter),
            new HBadgeSymbolPart(HBadgeSymbol.Nine, HBadgeColor.White, HBadgePosition.BottomRight)
        };

        Assert.Equal("s69110s70111s71112s72113s73114s74115s75116s76117s77118", badge.ToString());
    }

    [Fact]
    public void HBadge_ToString_BadgeWithAllPartTypes()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Ring, HBadgeColor.Black),
            new HBadgeSymbolPart(HBadgeSymbol.Sphere, HBadgeColor.White, HBadgePosition.Center),
            new HBadgeSymbolPart(HBadgeSymbol.Bobba, HBadgeColor.None, HBadgePosition.Center)
        };

        Assert.Equal("b22130t48114s06004", badge.ToString());
    }

    [Fact]
    public void HBadge_TryFormat_ShouldReturnFalseWhenTooSmallDestination()
    {
        Span<char> destination = stackalloc char[4];

        var badge = new HBadge
        {
            new HBadgeSymbolPart(HBadgeSymbol.Bobba, HBadgeColor.Black, HBadgePosition.Center)
        };

        Assert.False(badge.TryFormat(destination, out int charsWritten));
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void HBadge_ToString_EmptyBadgeShouldReturnEmptyString()
    {
        Assert.Equal(string.Empty, new HBadge().ToString());
    }

    [Fact]
    public void HBadge_TryParse_BadgeWithAllPartTypes_IsCorrect()
    {
        Assert.True(HBadge.TryParse("b22130t48114s06004", out var badge));

        Assert.Equal(3, badge.Count);

        Assert.IsType<HBadgeBasePart>(badge[0]);
        Assert.Equal(HBadgeBase.Ring, ((HBadgeBasePart)badge[0]).Type);
        Assert.Equal(HBadgeColor.Black, badge[0].Color);

        Assert.IsType<HBadgeSymbolPart>(badge[1]);
        Assert.Equal(HBadgeSymbol.Sphere, ((HBadgeSymbolPart)badge[1]).Symbol);
        Assert.Equal(HBadgeColor.White, badge[1].Color);

        Assert.IsType<HBadgeSymbolPart>(badge[2]);
        Assert.Equal(HBadgeSymbol.Bobba, ((HBadgeSymbolPart)badge[2]).Symbol);
        Assert.Equal(HBadgeColor.None, badge[2].Color);
    }

    [Fact]
    public void HBadge_TryParse_EmptyString_ReturnsEmptyBadge()
    {
        Assert.True(HBadge.TryParse(string.Empty, out var badge));
        Assert.Empty(badge);
    }

    [Fact]
    public void HBadge_TryParse_ToString_ReturnsSame()
    {
        const string AllPartTypes = "b22130t48114s06004";

        Assert.True(HBadge.TryParse(AllPartTypes, out var badge));
        Assert.Equal(AllPartTypes, badge.ToString());
    }

    [Fact]
    public void HBadge_Parse_ShouldThrowForInvalidArgumentValue()
    {
        Assert.Throws<ArgumentException>(() => HBadge.Parse("huuhaa"));
    }
}