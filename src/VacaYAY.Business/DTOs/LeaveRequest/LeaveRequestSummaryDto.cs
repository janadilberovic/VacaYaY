namespace VacaYAY.Business.DTOs.LeaveRequest;

/// <summary>Whole-table aggregates for the HR dashboard, which can't derive them from one page.</summary>
public class LeaveRequestSummaryDto
{
    public int TotalCount { get; set; }

    public IReadOnlyDictionary<string, int> CountByStatus { get; set; } = new Dictionary<string, int>();

    public IReadOnlyList<LeaveTypeDaysDto> DaysByType { get; set; } = [];
}

public class LeaveTypeDaysDto
{
    public int LeaveTypeId { get; set; }

    public LeaveTypeName LeaveTypeName { get; set; }

    public int WorkingDays { get; set; }
}
