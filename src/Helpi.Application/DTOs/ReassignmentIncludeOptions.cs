namespace Helpi.Application.DTOs;

public class ReassignmentIncludeOptions
{
    public bool IncludeJobInstance { get; set; }
    public bool IncludeScheduleAssignment { get; set; }
    public bool IncludeOrderSchedule { get; set; }
    public bool IncludeOrder { get; set; }
    public bool IncludeRequestedByUser { get; set; }
    public bool IncludeOriginalStudent { get; set; }
    public bool IncludeNewStudent { get; set; }
    public bool IncludeJobRequests { get; set; }
}