using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class Order
    {
        public int Id { get; set; }
        public int SeniorId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending; public bool IsRecurring { get; set; }
        public int? PaymentMethodId { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string? Notes { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? HangFireMatchingJobId { get; set; }

        public Senior Senior { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = null!;


        public ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
        public ICollection<OrderSchedule> Schedules { get; set; } = new List<OrderSchedule>();
    }
}
