using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class HNotificationDto
{
    public int Id { get; set; }
    public int RecieverUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string TranslationKey { get; set; } = null!;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? Payload { get; set; }

    public int? StudentId { get; set; }
    public StudentDto? Student { get; set; }
    public int? SeniorId { get; set; }
    public SeniorDto? Senior { get; set; }

    public int? OrderId { get; set; }
    public int? OrderScheduleId { get; set; }
    public int? JobInstanceId { get; set; }
}

public class CreateHNotificationDto
{
    public int RecieverUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string? Payload { get; set; }
}

public class UpdateHNotificationDto
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public NotificationType? Type { get; set; }
    public bool? IsRead { get; set; }
    public string? Payload { get; set; }
}

public class PagedHNotificationDto
{
    public IEnumerable<HNotificationDto> Notifications { get; set; } = new List<HNotificationDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ArchiveNotificationsResultDto
{
    public int ArchivedCount { get; set; }
    public string DriveFileUrl { get; set; } = "";
}