using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs.Order;


public class OrderScheduleDto
{
    public int Id { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsCancelled { get; set; }
}

public class OrderScheduleCreateDto
{
    [Range(1, 7)]
    public byte DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}

public class OrderScheduleUpdateDto
{
    public bool? IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
}