using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PricingConfigurationController : ControllerBase
{
    private readonly PricingConfigurationService _service;

    public PricingConfigurationController(PricingConfigurationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _service.GetAllConfigurationsAsync();
        return Ok(configs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var config = await _service.GetConfigurationByIdAsync(id);
        if (config == null) return NotFound();

        return Ok(config);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PricingConfigurationDto dto)
    {
        await _service.AddConfigurationAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(PricingConfigurationDto configDto, int changedBy, string reason = "")
    {


        await _service.UpdateConfigurationAsync(configDto, 1, reason);
        return NoContent();
    }


}
