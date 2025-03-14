using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs.Order;

public class OrderServiceCreateDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal HourlyRate { get; set; }

    [Required]
    [Range(0.1, double.MaxValue)]
    public decimal ScheduledHours { get; set; }
}

public class OrderServiceDto
{
    public int OrderId { get; set; }
    public int ServiceId { get; set; }
    public ServiceDto Service { get; set; } = null!;
    public decimal HourlyRate { get; set; }
    public decimal ScheduledHours { get; set; }
}