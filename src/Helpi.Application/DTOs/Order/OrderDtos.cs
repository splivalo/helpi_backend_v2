using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int SeniorId { get; set; }
    public OrderStatus Status { get; set; }

    public string? SeniorName { get; set; }
    public string? SeniorEmail { get; set; }
    public string? SeniorPhone { get; set; }
    public string? SeniorAddress { get; set; }

    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
    public int? PaymentMethodId { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ServiceDto> Services { get; set; } = new List<ServiceDto>();
    public ICollection<OrderScheduleDto> Schedules { get; set; } = new List<OrderScheduleDto>();
}

public class OrderCreateDto
{
    [Required]
    public int SeniorId { get; set; }

    public bool IsRecurring { get; set; }

    public int? PaymentMethodId { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string? Notes { get; set; }

    public ICollection<OrderServiceCreateDto> Services { get; set; } = new List<OrderServiceCreateDto>();
    public ICollection<OrderScheduleCreateDto> Schedules { get; set; } = new List<OrderScheduleCreateDto>();
}

public class OrderUpdateDto
{
    /// TODO: CREATE VALICATOR -SHOULD PREVENT CHANGING EDITING CANCELED SCHEDULE
    public int? PaymentMethodId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public OrderStatus? Status { get; set; }
    /// <summary>
    /// Set to 0 to remove promo code, or a positive int to set/change promo code.
    /// Null means no change.
    /// </summary>
    public int? PromoCodeId { get; set; }

    public ICollection<OrderServiceCreateDto> ServicesToAdd { get; set; } = new List<OrderServiceCreateDto>();
    public ICollection<int> ServiceIdsToRemove { get; set; } = new List<int>();
    public ICollection<OrderScheduleCreateDto> SchedulesToAdd { get; set; } = new List<OrderScheduleCreateDto>();
    public ICollection<int> ScheduleIdsToRemove { get; set; } = new List<int>();
}

public class OrderCancelDto
{
    [Required]
    public string CancellationReason { get; set; } = null!;
}
