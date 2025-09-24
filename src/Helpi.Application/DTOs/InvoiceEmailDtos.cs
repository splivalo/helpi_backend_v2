using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class InvoiceEmailDto
{
    public int Id { get; set; }
    public EmailStatus Status { get; set; }
    public DateTime? LastAttempt { get; set; }
}

public class InvoiceEmailUpdateDto
{
    public EmailStatus? Status { get; set; }
}