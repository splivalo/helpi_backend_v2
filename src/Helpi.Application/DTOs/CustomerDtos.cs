using System.ComponentModel.DataAnnotations;

using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

// CustomerDtos.cs
public class CustomerDto
{
    public int UserId { get; set; }
    public NotificationMethod PreferredNotificationMethod { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;

    public ICollection<SeniorDto> Seniors { get; set; } = new List<SeniorDto>();
}

public class CustomerCreateDto
{
    [Required]
    public int ContactId { get; set; }

    public NotificationMethod PreferredNotificationMethod { get; set; } = NotificationMethod.Email;
}

public class CustomerUpdateDto
{
    public NotificationMethod? PreferredNotificationMethod { get; set; }
}