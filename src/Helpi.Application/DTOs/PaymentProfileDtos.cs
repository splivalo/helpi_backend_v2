// PaymentProfileDto.cs
using Helpi.Domain.Enums;

public class PaymentProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? StripeCustomerId { get; set; }

    public PaymentProcessor PaymentProcessor { get; set; }
    public string? StripeConnectAccountId { get; set; }
    public bool IsPayoutEnabled { get; set; }
    public DateTime? LastPayoutDate { get; set; }
    public string? DefaultPaymentMethodId { get; set; }
    public string? PreferredPayoutMethod { get; set; }
}

// CreatePaymentProfileDto.cs
public class CreatePaymentProfileDto
{
    public int UserId { get; set; }
    public string? StripeCustomerId { get; set; }

    public PaymentProcessor PaymentProcessor { get; set; }
    public string? StripeConnectAccountId { get; set; }
}

// UpdatePaymentProfileDto.cs
public class UpdatePaymentProfileDto
{
    public bool IsPayoutEnabled { get; set; }
    public string? DefaultPaymentMethodId { get; set; }
    public string? PreferredPayoutMethod { get; set; }
    public DateTime? LastPayoutDate { get; set; }
}
