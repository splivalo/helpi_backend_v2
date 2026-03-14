using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class SuspensionLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SuspensionAction Action { get; set; }
    public string? Reason { get; set; }
    public int AdminId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SuspendUserDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = null!;
}

public class UserSuspensionStatusDto
{
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public int? SuspendedByAdminId { get; set; }
    public List<SuspensionLogDto> SuspensionHistory { get; set; } = new();
}
