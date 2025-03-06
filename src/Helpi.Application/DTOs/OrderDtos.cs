using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ServiceDto Service { get; set; } = null!;
}

public class OrderCreateDto
{
    [Required]
    public int SeniorId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class OrderUpdateDto
{
    public OrderStatus? Status { get; set; }
    public DateOnly? EndDate { get; set; }
}
