using System.Reflection;
using VacaYAY.Business.Services.LeaveRequest;
using Xunit;
namespace VacaYAY.UnitTests;

public class SerbianHolidayProviderTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(1, 7)]
    [InlineData(2, 15)]
    [InlineData(2, 16)]
    [InlineData(5, 1)]
    [InlineData(5, 2)]
    [InlineData(11, 11)]
    public void IsHoliday_ReturnsTrue_ForNewYear(int month,int day)
    {
        // Given
        var provider= new SerbianHolidayProvider();
        // When
        var result=provider.IsHoliday(new DateOnly(2026,month,day));

        // Then
        Assert.True(result);
    }
    [Fact]
    public void IsHoliday_ReturnsFalse_ForWorkingDay()
    {
        // Given
        var provider= new SerbianHolidayProvider();

        // When
         var result=provider.IsHoliday(new DateOnly(2026,2,1));
        // Then
        Assert.False(result);
    }
    [Theory]
    [InlineData(2024,5,5)]
    [InlineData(2024,5,3)]
    [InlineData(2024,5,4)]
    [InlineData(2025,4,20)]
    [InlineData(2026,4,12)]
    public void IsEaster_ReturnsTrue(int year,int month,int day)
    {
        // Given
        var provider=new SerbianHolidayProvider();
        // When
        var holidays=provider.ForYear(year);
        // Then
        Assert.Contains(new DateOnly(year,month,day),holidays);
    }

}