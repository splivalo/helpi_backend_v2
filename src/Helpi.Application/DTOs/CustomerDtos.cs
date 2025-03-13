using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

// CustomerDtos.cs
public class CustomerDto
{
    public int Id { get; set; }
    public NotificationMethod PreferredNotificationMethod { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;

    public ICollection<Senior> Seniors { get; set; } = new List<Senior>();
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