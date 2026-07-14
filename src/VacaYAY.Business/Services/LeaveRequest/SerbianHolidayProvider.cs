using System.Collections.Concurrent;
using VacaYAY.Business.Interfaces.LeaveRequest;

namespace VacaYAY.Business.Services.LeaveRequest;


public class SerbianHolidayProvider : IHolidayProvider
{
    private readonly ConcurrentDictionary<int, IReadOnlySet<DateOnly>> _cache = new();

    public bool IsHoliday(DateOnly date) => ForYear(date.Year).Contains(date);

    public IReadOnlySet<DateOnly> ForYear(int year) => _cache.GetOrAdd(year, Build);

    private static IReadOnlySet<DateOnly> Build(int year)
    {
        var days = new HashSet<DateOnly>
        {
            new(year, 1, 1),
            new(year, 1, 2),
            new(year, 1, 7),
            new(year, 2, 15),
            new(year, 2, 16),
            new(year, 5, 1),
            new(year, 5, 2),
            new(year, 11, 11),
        };
        
        var easterSunday = OrthodoxEaster(year);

        days.Add(easterSunday.AddDays(-2)); // Good Friday
        days.Add(easterSunday.AddDays(-1)); // Holy Saturday
        days.Add(easterSunday);             // Easter Sunday
        days.Add(easterSunday.AddDays(1));  // Easter Monday

        return days;
    }


    private static DateOnly OrthodoxEaster(int year)
    {
        int a = year % 4;
        int b = year % 7;
        int c = year % 19;
        int d = (19 * c + 15) % 30;
        int e = (2 * a + 4 * b - d + 34) % 7;
        int month = (d + e + 114) / 31;       // 3 = March, 4 = April
        int day = ((d + e + 114) % 31) + 1;

        return new DateOnly(year, month, day).AddDays(13);
    }
}
