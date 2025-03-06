using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class PaymentMethod
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public PaymentProcessor Processor { get; set; }

        [MaxLength(255)]
        public string Token { get; set; } = null!;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer Customer { get; set; } = null!;
    }
}
