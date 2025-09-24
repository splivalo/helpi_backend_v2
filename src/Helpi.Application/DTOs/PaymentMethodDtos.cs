using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class PaymentMethodDto
{
    public int Id { get; set; }
    public int UserId { get; set; } // customer or student
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public PaymentProcessor Processor { get; set; }  // "Stripe", "PayPal", etc.
    public string? ProcessorToken { get; set; }  // Eg Stripe PaymentMethodID 

    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

}

public class PaymentMethodCreateDto
{

    [Required]
    public int UserId { get; set; } // customer or student
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    [Required]
    public PaymentProcessor Processor { get; set; }  // "Stripe", "PayPal", etc.
    public string? ProcessorToken { get; set; }  // Eg Stripe PaymentMethodID 

    public bool IsDefault { get; set; } = false;


}

public class PaymentMethodUpdateDto
{
    public bool? IsDefault { get; set; }
}


public class SaveStripePaymentMethodDto
{
    public string PaymentMethodId { get; set; } = null!;
}

public class StripeSetupIntentResponseDto
{
    public string ClientSecret { get; set; } = null!;
}
