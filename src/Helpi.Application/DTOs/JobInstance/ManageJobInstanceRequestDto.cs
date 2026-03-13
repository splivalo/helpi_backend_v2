namespace Helpi.Application.DTOs.JobInstance;

public class ManageSessionRequestDto
{
    public DateOnly? NewDate { get; set; }
    public TimeOnly? NewStartTime { get; set; }
    public TimeOnly? NewEndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? PreferredStudentId { get; set; }
    public bool ReassignStudent { get; set; } = true;
    public int RequestedByUserId { get; set; } = 1;
}
