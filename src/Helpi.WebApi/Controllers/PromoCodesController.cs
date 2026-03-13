using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/promo-codes")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _promoCodeService;

    public PromoCodesController(IPromoCodeService promoCodeService)
    {
        _promoCodeService = promoCodeService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<PromoCodeDto>>> GetAll()
    {
        return Ok(await _promoCodeService.GetAllAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PromoCodeDto>> GetById(int id)
    {
        var result = await _promoCodeService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<PromoCodeDto>> Create([FromBody] PromoCodeCreateDto dto)
    {
        var result = await _promoCodeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<PromoCodeDto>> Update(int id, [FromBody] PromoCodeUpdateDto dto)
    {
        var result = await _promoCodeService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _promoCodeService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("validate")]
    public async Task<ActionResult<PromoCodeValidationResultDto>> Validate(
        [FromQuery] string code,
        [FromQuery] int customerId,
        [FromQuery] decimal orderTotal)
    {
        var result = await _promoCodeService.ValidateCodeAsync(code, customerId, orderTotal);
        return Ok(result);
    }

    [HttpPost("apply")]
    public async Task<ActionResult<PromoCodeUsageDto>> Apply(
        [FromQuery] string code,
        [FromQuery] int orderId,
        [FromQuery] int customerId,
        [FromQuery] decimal orderTotal)
    {
        var result = await _promoCodeService.ApplyCodeAsync(code, orderId, customerId, orderTotal);
        return Ok(result);
    }
}
