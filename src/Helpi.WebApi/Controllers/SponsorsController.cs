using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorsController : ControllerBase
{
    private readonly ISponsorService _sponsorService;
    private readonly IWebHostEnvironment _env;

    public SponsorsController(ISponsorService sponsorService, IWebHostEnvironment env)
    {
        _sponsorService = sponsorService;
        _env = env;
    }

    /// <summary>
    /// Get active sponsors (public — used by mobile app).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<List<SponsorDto>>> GetActive()
    {
        return Ok(await _sponsorService.GetActiveAsync());
    }

    /// <summary>
    /// Get all sponsors (admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<SponsorDto>>> GetAll()
    {
        return Ok(await _sponsorService.GetAllAsync());
    }

    /// <summary>
    /// Get sponsor by ID (admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorDto>> GetById(int id)
    {
        var result = await _sponsorService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Create a new sponsor (admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SponsorDto>> Create([FromBody] SponsorCreateDto dto)
    {
        var result = await _sponsorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a sponsor (admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<SponsorDto>> Update(int id, [FromBody] SponsorUpdateDto dto)
    {
        var result = await _sponsorService.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Upload logo for a sponsor (admin only). Variant: "light" or "dark".
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file, [FromQuery] string variant = "light")
    {
        try
        {
            var url = await _sponsorService.UploadLogoAsync(id, file, variant, _env.WebRootPath);
            return Ok(new { logoUrl = url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete logo for a sponsor (admin only). Variant: "light" or "dark".
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/logo")]
    public async Task<IActionResult> DeleteLogo(int id, [FromQuery] string variant = "light")
    {
        try
        {
            await _sponsorService.DeleteLogoAsync(id, variant, _env.WebRootPath);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a sponsor (admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sponsorService.DeleteAsync(id);
        return NoContent();
    }
}
