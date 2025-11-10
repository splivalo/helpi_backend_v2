namespace Helpi.Application.DTOs;

public class JobInstanceIncludeOptions
{
    public bool Senior { get; set; }
    public bool Assignment { get; set; }
    public bool AssignmentStudent { get; set; }
    public bool AssignmentOrderSchedule { get; set; }
    public bool PrevAssignment { get; set; }
    public bool PrevAssignmentOrderSchedule { get; set; }
    public bool Order { get; set; }
    public bool OrderSchedule { get; set; }
    public bool OrderPaymentMethod { get; set; }
    public bool OrderServices { get; set; }

}