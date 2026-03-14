using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/suspensions")]
public class SuspensionsController : ControllerBase
{
    private readonly SuspensionService _service;

    public SuspensionsController(SuspensionService service)
    {
        _service = service;
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetSuspensionStatus(int userId)
    {
        var status = await _service.GetSuspensionStatusAsync(userId);
        return Ok(status);
    }

    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(int userId, [FromBody] SuspendUserDto dto)
    {
        var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminIdString) || !int.TryParse(adminIdString, out var adminId))
            return Unauthorized();

        var log = await _service.SuspendUserAsync(userId, dto.Reason, adminId);
        return Ok(log);
    }

    [HttpPost("users/{userId}/activate")]
    public async Task<IActionResult> ActivateUser(int userId)
    {
        var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminIdString) || !int.TryParse(adminIdString, out var adminId))
            return Unauthorized();

        var log = await _service.ActivateUserAsync(userId, adminId);
        return Ok(log);
    }
}
