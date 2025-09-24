using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PricingChangeHistoryController : ControllerBase
{
    private readonly PricingChangeHistoryService _historyService;

    public PricingChangeHistoryController(PricingChangeHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var history = await _historyService.GetAllAsync();
        return Ok(history);
    }

    [HttpGet("config/{configId}")]
    public async Task<IActionResult> GetByConfigurationId(int configId)
    {
        var history = await _historyService.GetByConfigurationIdAsync(configId);
        return Ok(history);
    }
}
