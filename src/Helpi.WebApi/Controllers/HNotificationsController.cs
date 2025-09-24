using Microsoft.AspNetCore.Mvc;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;

namespace Helpi.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HNotificationsController : ControllerBase
{
    private readonly IHNotificationService _notificationService;

    public HNotificationsController(IHNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HNotificationDto>> GetById(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<HNotificationDto>>> GetByUserId(int userId)
    {
        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread")]
    public async Task<ActionResult<IEnumerable<HNotificationDto>>> GetUnreadByUserId(int userId)
    {
        var notifications = await _notificationService.GetUnreadByUserIdAsync(userId);
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
        [FromQuery] int pageSize = 10)
    {
        var result = await _notificationService.GetPagedAsync(userId, page, pageSize);
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

    [HttpPatch("{id}/mark-read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("user/{userId}/mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead(int userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}