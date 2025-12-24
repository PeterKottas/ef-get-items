using System.Reflection;

namespace EntityFramework.Extensions.GetItems.UnitTests;

public class ParsingTests
{
    // Access the private ParseSingleValue method via reflection
    private static object? ParseSingleValue(string? value, Type type)
    {
        var method = typeof(GetItemsExtension).GetMethod("ParseSingleValue", BindingFlags.NonPublic | BindingFlags.Static);
        return method?.Invoke(null, [value, type]);
    }

    private static Guid ParseFlexibleGuid(string value)
    {
        var method = typeof(GetItemsExtension).GetMethod("ParseFlexibleGuid", BindingFlags.NonPublic | BindingFlags.Static);
        return (Guid)method?.Invoke(null, [value])!;
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("-456", -456)]
    [InlineData("0", 0)]
    public void ParseSingleValue_Int(string input, int expected)
    {
        var result = ParseSingleValue(input, typeof(int));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123456789012345", 123456789012345L)]
    public void ParseSingleValue_Long(string input, long expected)
    {
        var result = ParseSingleValue(input, typeof(long));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseSingleValue_Double()
    {
        // Note: Convert.ToDouble uses current culture, so we test with culture-invariant expected results
        var result = ParseSingleValue("314", typeof(double));
        Assert.Equal(314.0, result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("False", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    public void ParseSingleValue_Bool(string input, bool expected)
    {
        var result = ParseSingleValue(input, typeof(bool));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseSingleValue_DateTime()
    {
        var result = ParseSingleValue("2024-06-15T10:30:00Z", typeof(DateTime));
        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void ParseSingleValue_DateOnly()
    {
        var result = ParseSingleValue("2024-06-15", typeof(DateOnly));
        Assert.IsType<DateOnly>(result);
        Assert.Equal(new DateOnly(2024, 6, 15), result);
    }

    [Fact]
    public void ParseSingleValue_Guid_StandardFormat()
    {
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var result = ParseSingleValue("12345678-1234-1234-1234-123456789abc", typeof(Guid));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseSingleValue_Enum()
    {
        var result = ParseSingleValue("CategoryA", typeof(TestCategoryEnum));
        Assert.Equal(TestCategoryEnum.CategoryA, result);
    }

    [Fact]
    public void ParseSingleValue_String_ReturnsAsIs()
    {
        var result = ParseSingleValue("hello world", typeof(string));
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void ParseSingleValue_Null_ReturnsNull()
    {
        var result = ParseSingleValue(null, typeof(int));
        Assert.Null(result);
    }

    [Fact]
    public void ParseSingleValue_Decimal()
    {
        // Note: Convert.ToDecimal uses current culture
        var result = ParseSingleValue("123", typeof(decimal));
        Assert.Equal(123m, result);
    }

    [Fact]
    public void ParseFlexibleGuid_StandardFormat()
    {
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var result = ParseFlexibleGuid("12345678-1234-1234-1234-123456789abc");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseFlexibleGuid_NoHyphens()
    {
        var expected = Guid.Parse("12345678-1234-1234-1234-1234567890ab");
        var result = ParseFlexibleGuid("123456781234123412341234567890ab");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseFlexibleGuid_ShortGuid_PadsWithZeros()
    {
        var result = ParseFlexibleGuid("1234");
        Assert.Equal(Guid.Parse("12340000-0000-0000-0000-000000000000"), result);
    }

    [Fact]
    public void ParseFlexibleGuid_Empty_ReturnsEmptyGuid()
    {
        var result = ParseFlexibleGuid("");
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void ParseFlexibleGuid_WithExtraChars_IgnoresNonHex()
    {
        var result = ParseFlexibleGuid("{1234-5678}");
        // Should parse hex chars: 12345678 and pad with zeros
        Assert.NotEqual(Guid.Empty, result);
    }
}

