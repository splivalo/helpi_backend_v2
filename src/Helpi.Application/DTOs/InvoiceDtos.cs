using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public InvoiceStatus Status { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? SentAt { get; set; }
}

public class InvoiceUpdateDto
{
    public InvoiceStatus? Status { get; set; }
}