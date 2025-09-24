
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/fcm-tokens")]
public class FcmTokensController : ControllerBase
{
    private readonly FcmTokensService _service;

    public FcmTokensController(FcmTokensService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<FcmToken>>> GetUserTokensByUserId(int userId)
    {
        var tokens = await _service.GetUserFcmTokens(userId);
        return Ok(tokens);
    }
    [HttpPost]
    public async Task<ActionResult<FcmToken>> Create(FcmToken fcmToken)
    {
        var token = await _service.CreateFcmTokenAsync(fcmToken);

        return CreatedAtAction(nameof(GetUserTokensByUserId), token);
    }
}