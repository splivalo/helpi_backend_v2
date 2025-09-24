namespace Helpi.Application.DTOs;

public class OrderIncludeOptions
{
    public bool Senior { get; set; }
    public bool OrderServices { get; set; }
    public bool Schedules { get; set; }

    public bool SchedulesJobRequests { get; internal set; }
    public bool ScheduleAssignments { get; set; }
    public bool AssignmentsJobInstances { get; set; }


}