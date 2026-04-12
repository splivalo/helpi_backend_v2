using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }
    public int SeniorId { get; set; }
    public OrderStatus Status { get; set; }

    public string? SeniorName { get; set; }
    public string? SeniorEmail { get; set; }
    public string? SeniorPhone { get; set; }
    public string? SeniorAddress { get; set; }
    public decimal? SeniorLatitude { get; set; }
    public decimal? SeniorLongitude { get; set; }

    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
    public int? PaymentMethodId { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssignedStudentName { get; set; }
    public int? AssignedStudentId { get; set; }
    public string? AssignedStudentEmail { get; set; }
    public string? AssignedStudentPhone { get; set; }
    public string? AssignedStudentAddress { get; set; }
    public string? AssignedStudentCity { get; set; }
    public int? AssignedStudentFaculty { get; set; }
    public DateOnly? AssignedStudentDateOfBirth { get; set; }
    public int? AssignedStudentGender { get; set; }
    public int? AssignedStudentStatus { get; set; }
    public decimal? AssignedStudentAverageRating { get; set; }
    public int? AssignedStudentTotalReviews { get; set; }
    public int? AssignedStudentDaysToContractExpire { get; set; }
    public int? PromoCodeId { get; set; }
    public string? PromoCodeCode { get; set; }
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
