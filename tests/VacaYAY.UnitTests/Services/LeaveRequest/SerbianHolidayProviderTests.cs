using VacaYAY.Business.Services.LeaveRequest;

namespace VacaYAY.UnitTests;

public class SerbianHolidayProviderTests
{
    private readonly SerbianHolidayProvider _provider = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(1, 7)]
    [InlineData(2, 15)]
    [InlineData(2, 16)]
    [InlineData(5, 1)]
    [InlineData(5, 2)]
    [InlineData(11, 11)]
    public void IsHoliday_ReturnsTrue_ForNewYear(int month, int day)
    {
        // When
        var result = _provider.IsHoliday(new DateOnly(2026, month, day));

        // Then
        Assert.True(result);
    }

    [Fact]
    public void IsHoliday_ReturnsFalse_ForWorkingDay()
    {
        // When
        var result = _provider.IsHoliday(new DateOnly(2026, 2, 1));

        // Then
        Assert.False(result);
    }

    [Theory]
    [InlineData(2024, 5, 5)]
    [InlineData(2024, 5, 3)]
    [InlineData(2024, 5, 4)]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 12)]
    public void IsEaster_ReturnsTrue(int year, int month, int day)
    {
        // When
        var holidays = _provider.ForYear(year);

        // Then
        Assert.Contains(new DateOnly(year, month, day), holidays);
    }
}
