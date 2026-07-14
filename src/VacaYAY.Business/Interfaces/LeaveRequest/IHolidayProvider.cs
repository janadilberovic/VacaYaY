namespace VacaYAY.Business.Interfaces.LeaveRequest;

public interface IHolidayProvider
{
    bool IsHoliday(DateOnly date);

    IReadOnlySet<DateOnly> ForYear(int year);
}
