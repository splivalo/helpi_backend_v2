using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Infrastructure.Services;

namespace Helpi.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HNotificationsController : ControllerBase
{
    private readonly IHNotificationService _notificationService;
    private readonly IGoogleDriveService _driveService;
    private readonly GoogleDriveSettings _driveSettings;

    public HNotificationsController(
        IHNotificationService notificationService,
        IGoogleDriveService driveService,
        IOptions<GoogleDriveSettings> driveSettings)
    {
        _notificationService = notificationService;
        _driveService = driveService;
        _driveSettings = driveSettings.Value;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HNotificationDto>> GetById(int id, [FromQuery] string languageCode = "en")
    {
        var notification = await _notificationService.GetByIdAsync(id, languageCode);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<HNotificationDto>>> GetByUserId(int userId,
    [FromQuery] string languageCode = "en")
    {
        var notifications = await _notificationService.GetByUserIdAsync(userId, languageCode);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread")]
    public async Task<ActionResult<IEnumerable<HNotificationDto>>> GetUnreadByUserId(int userId,
     [FromQuery] string languageCode = "en")
    {
        var notifications = await _notificationService.GetUnreadByUserIdAsync(userId, languageCode);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(int userId)
    {
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpGet("user/{userId}/paged")]
    public async Task<ActionResult<PagedHNotificationDto>> GetPaged(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string languageCode = "en"
        )
    {
        var result = await _notificationService.GetPagedAsync(userId, page, pageSize, languageCode);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HNotificationDto>> Create([FromBody] CreateHNotificationDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var notification = await _notificationService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HNotificationDto>> Update(int id, [FromBody] UpdateHNotificationDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var notification = await _notificationService.UpdateAsync(id, updateDto);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _notificationService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/mark-read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("user/{userId}/mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead(int userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    [HttpPost("user/{userId}/archive")]
    public async Task<ActionResult<ArchiveNotificationsResultDto>> ArchiveReadNotifications(
        int userId,
        [FromQuery] string languageCode = "hr")
    {
        var folderId = _driveSettings.NotificationsArchiveFolderId;
        if (string.IsNullOrEmpty(folderId))
            return BadRequest("NotificationsArchiveFolderId is not configured.");

        var readNotifications = await _notificationService.GetReadByUserIdAsync(userId, languageCode);
        var notifList = readNotifications.ToList();

        if (notifList.Count == 0)
            return Ok(new ArchiveNotificationsResultDto { ArchivedCount = 0 });

        // Build new CSV rows (no header — appended to existing file)
        var sb = new StringBuilder();
        foreach (var n in notifList)
        {
            var date = n.CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            var title = EscapeCsv(n.Title);
            var body = EscapeCsv(n.Body);
            sb.AppendLine($"{date},{title},{body}");
        }
        var newRows = sb.ToString();

        const string archiveFileName = "notifications-archive.csv";
        const string header = "Datum,Naslov,Poruka";
        string driveUrl;

        // Try to find existing master file
        var existingFileId = await _driveService.FindFileInFolderAsync(folderId, archiveFileName);

        if (existingFileId != null)
        {
            // Download existing content, append new rows, update
            var existingBytes = await _driveService.DownloadFileAsync(existingFileId);
            var existingContent = Encoding.UTF8.GetString(existingBytes).TrimEnd();

            // Strip BOM if present
            if (existingContent.Length > 0 && existingContent[0] == '\uFEFF')
                existingContent = existingContent[1..];

            var merged = existingContent + "\n" + newRows;
            var mergedBytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(merged)).ToArray();

            driveUrl = await _driveService.UpdateFileAsync(existingFileId, mergedBytes, "text/csv");
        }
        else
        {
            // Create new master file with header
            var fullCsv = header + "\n" + newRows;
            var csvBytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(fullCsv)).ToArray();

            driveUrl = await _driveService.UploadFileToFolderAsync(
                folderId, csvBytes, archiveFileName, "text/csv");
        }

        var deletedCount = await _notificationService.DeleteReadByUserIdAsync(userId);

        return Ok(new ArchiveNotificationsResultDto
        {
            ArchivedCount = deletedCount,
            DriveFileUrl = driveUrl
        });
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}