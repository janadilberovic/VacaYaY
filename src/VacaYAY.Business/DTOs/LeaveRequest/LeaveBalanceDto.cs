namespace VacaYAY.Business.DTOs.LeaveRequest;

public class LeaveBalanceDto
{
    public int DaysOff { get; set; }

    /// <summary>Working days already committed to pending requests, not yet deducted from <see cref="DaysOff"/>.</summary>
    public int PendingDays { get; set; }

    public int RemainingDays { get; set; }
}
