using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class HEmailDto
{
    public int Id { get; set; }
    public EmailStatus Status { get; set; }
    public DateTime? LastAttempt { get; set; }
}

public class HEmailUpdateDto
{
    public EmailStatus? Status { get; set; }
}