using Microsoft.AspNetCore.Mvc;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Helpi.Domain.Entities;
using Helpi.Application.DTOs;

namespace Helpi.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SignalRController : ControllerBase
{
    private readonly ISignalRNotificationService _signalRService;

    public SignalRController(ISignalRNotificationService signalRService)
    {
        _signalRService = signalRService;
    }

    [HttpPost("send-to-user/{userId}")]
    public async Task<IActionResult> SendToUser(int userId, [FromBody] HNotificationDto notification)
    {
        await _signalRService.SendNotificationToUserAsync(userId, notification);
        return Ok();
    }

    [HttpPost("send-to-group/{groupName}")]
    public async Task<IActionResult> SendToGroup(string groupName, [FromBody] HNotificationDto notification)
    {
        await _signalRService.SendNotificationToGroupAsync(groupName, notification);
        return Ok();
    }

    [HttpPost("broadcast-system")]
    public async Task<IActionResult> BroadcastSystem([FromBody] HNotificationDto notification)
    {
        await _signalRService.BroadcastSystemNotificationAsync(notification);
        return Ok();
    }
}