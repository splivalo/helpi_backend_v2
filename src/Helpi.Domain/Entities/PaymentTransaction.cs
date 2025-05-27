using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int JobInstanceId { get; set; }
        public int CustomerId { get; set; }
        public int PaymentMethodId { get; set; }

        // [Precision(18, 2)]
        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "EUR";
        public DateTime ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public byte RetryCount { get; set; } = 0;
        public byte MaxRetries { get; set; } = 3;

        [MaxLength(255)]
        public string? GatewayId { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonDocument? GatewayResponse { get; set; }

        [MaxLength(64)]
        public string? IdempotencyKey { get; set; }

        public JobInstance JobInstance { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = null!;
    }
}
