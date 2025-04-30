

using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class PaymentMethod
    {
        public int Id { get; set; }
        public int UserId { get; set; } // customer or student

        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }


        public PaymentProcessor PaymentProcessor { get; set; } // "Stripe", "PayPal", etc.
        public string? ProcessorToken { get; set; }  // Eg Stripe PaymentMethodID 

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsAcctive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public User User { get; set; } = null!; // customer or student

        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
