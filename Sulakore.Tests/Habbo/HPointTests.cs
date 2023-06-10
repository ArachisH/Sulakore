using Sulakore.Habbo;

using Xunit;

namespace Sulakore.Tests.Habbo;

public class HPointTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData(",")]
    [InlineData(",,")]
    [InlineData(",,,")]
    [InlineData(",,,,")]
    [InlineData("0,")]
    [InlineData(",0")]
    [InlineData("0.0")]
    [InlineData("0,0,")]
    [InlineData("0,,0")]
    [InlineData("0,0,0,")]
    public void HPoint_Parse_ShouldThrowFormatExceptionForInvalid(string input)
    {
        Assert.Throws<FormatException>(() => HPoint.Parse(input));
    }

    public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { "0,0",         new HPoint(0, 0) },
            new object[] { "4,2",         new HPoint(4, 2) },
            new object[] { "4, 2",        new HPoint(4, 2) },
            new object[] { "0,0,0",       new HPoint(0, 0) },
            new object[] { "4,2,0",       new HPoint(4, 2) },
            new object[] { " 4 , 2 , 0 ", new HPoint(4, 2) },
            new object[] { "4 ,2 ,0 ",    new HPoint(4, 2) },
            new object[] { " 4, 2, 0",    new HPoint(4, 2) },
            new object[] { "4,2,0.0",     new HPoint(4, 2) },
            
            // Z Equals under default epsilon (0.01f)
            new object[] { "0,0,0",     new HPoint(0, 0, 0f) },
            new object[] { "0,0,0.0",   new HPoint(0, 0, 0f) },
            new object[] { "0,0,-0",    new HPoint(0, 0, -0f) },
            new object[] { "0,0,-0.0",  new HPoint(0, 0, -0f) },
            new object[] { "1,2,3",     new HPoint(1, 2, 3f) },
            new object[] { "1,2,3.0",   new HPoint(1, 2, 3f) },
            new object[] { "1,2,3.005", new HPoint(1, 2, 3f) },
            new object[] { "1,2,-3",    new HPoint(1, 2, -3f) },
            new object[] { "1,2,-3.0",  new HPoint(1, 2, -3f) },
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public void HPoint_Parse_EqualsXY(string input, HPoint expected)
    {
        Assert.Equal(expected, HPoint.Parse(input));
    }


    [Theory]
    [MemberData(nameof(TestData))]
    public void HPoint_Parse_EqualsXYZ(string input, HPoint expected)
    {
        Assert.True(HPoint.TryParse(input, out HPoint actual));
        Assert.True(expected.Equals(actual, HPoint.DEFAULT_EPSILON));
    }
}
