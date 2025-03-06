using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class PaymentMethodDto
{
    public int Id { get; set; }
    public PaymentProcessor Processor { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentMethodCreateDto
{
    [Required]
    public PaymentProcessor Processor { get; set; }

    [Required]
    public string Token { get; set; } = null!;

    public bool IsDefault { get; set; }
}

public class PaymentMethodUpdateDto
{
    public bool? IsDefault { get; set; }
}