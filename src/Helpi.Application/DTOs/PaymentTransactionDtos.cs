using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class PaymentTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
}

public class PaymentTransactionUpdateDto
{
    public PaymentStatus? Status { get; set; }
    public DateTime? NextRetryAt { get; set; }
}