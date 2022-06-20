using Sulakore.Habbo.Badge;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HBadgeTests
{
    [Fact]
    public void HBadge_SingleLigthGrayTriangleBase_ToString()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Triangle, HBadgeColor.LightGray)
        };

        Assert.Equal("b03120", badge.ToString());
    }

    [Fact]
    public void HBadge_PurpleSquareBaseWithDarkRedSkullSymbol_ToString()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Square, HBadgeColor.Purple),
            new HBadgeSymbolPart(HBadgeSymbol.Bobba, HBadgeColor.DarkRed, HBadgePosition.Center)
        };

        Assert.Equal("b01200s06104", badge.ToString());
    }

    [Fact]
    public void HBadge_LightGreenTriangleOnTopOfSquare_ToString()
    {
        var badge = new HBadge
        {
            new HBadgeBasePart(HBadgeBase.Square, HBadgeColor.LightGreen),
            new HBadgeBasePart(HBadgeBase.Triangle, HBadgeColor.LightGreen)
        };

        Assert.Equal("b01030b03030", badge.ToString());
    }

    [Fact]
    public void HBadge_NumberLetterSymbols_ToString()
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
    public void HBadge_Empty_ToString_ShouldProduceEmptyString()
    {
        Assert.Equal(string.Empty, new HBadge().ToString());
    }
}
