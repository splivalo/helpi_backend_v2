using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class PaymentProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? MinimaxCustomerId { get; set; }

        public PaymentProcessor PaymentProcessor { get; set; }

        // For making payments
        public string? StripeCustomerId { get; set; }

        // For receiving payments (if using Stripe Connect)
        public string? StripeConnectAccountId { get; set; }
        public bool IsPayoutEnabled { get; set; }

        // Additional payment-related properties
        public DateTime? LastPayoutDate { get; set; }
        public string? DefaultPaymentMethodId { get; set; }
        public string? PreferredPayoutMethod { get; set; }
    }
}