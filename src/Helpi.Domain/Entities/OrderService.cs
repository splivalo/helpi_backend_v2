using Helpi.Domain.Entities;


namespace Helpi.Domain.Entities;
public class OrderService
{
    public int OrderId { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public decimal HourlyRate { get; set; }
    public decimal ScheduledHours { get; set; }
}