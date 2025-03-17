using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ICollection<ServiceDto> Services { get; set; } = new List<ServiceDto>();
    public ICollection<OrderScheduleDto> Schedules { get; set; } = new List<OrderScheduleDto>();
}

public class OrderCreateDto
{
    [Required]
    public int SeniorId { get; set; }

    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public ICollection<OrderServiceCreateDto> Services { get; set; } = new List<OrderServiceCreateDto>();
    public ICollection<OrderScheduleCreateDto> Schedules { get; set; } = new List<OrderScheduleCreateDto>();
}

public class OrderUpdateDto
{
    public OrderStatus? Status { get; set; }
    public DateOnly? EndDate { get; set; }
}
