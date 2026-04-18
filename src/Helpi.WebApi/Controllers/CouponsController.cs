using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    #region CRUD (Admin only)

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetAll()
    {
        return Ok(await _couponService.GetAllAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CouponDto>> GetById(int id)
    {
        var result = await _couponService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create([FromBody] CouponCreateDto dto)
    {
        var result = await _couponService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CouponDto>> Update(int id, [FromBody] CouponUpdateDto dto)
    {
        var result = await _couponService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Assignments

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<CouponAssignmentDto>> AssignToSenior(int id, [FromBody] CouponAssignDto dto)
    {
        var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? adminId = int.TryParse(adminIdString, out var aid) ? aid : null;

        var result = await _couponService.AssignToSeniorAsync(id, dto.SeniorId, adminId);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}/assignments")]
    public async Task<ActionResult<List<CouponAssignmentDto>>> GetAssignments(int id)
    {
        var result = await _couponService.GetAssignmentsByCouponIdAsync(id);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("assignments/{assignmentId}")]
    public async Task<IActionResult> DeactivateAssignment(int assignmentId)
    {
        await _couponService.DeactivateAssignmentAsync(assignmentId);
        return NoContent();
    }

    #endregion

    #region Senior endpoints

    [HttpPost("redeem")]
    public async Task<ActionResult<CouponRedeemResultDto>> Redeem([FromBody] CouponRedeemDto dto)
    {
        var result = await _couponService.RedeemAsync(dto.Code, dto.SeniorId);
        return Ok(result);
    }

    [HttpGet("my-coupons")]
    public async Task<ActionResult<List<CouponAssignmentDto>>> GetMyCoupons([FromQuery] int seniorId)
    {
        var result = await _couponService.GetAssignmentsForSeniorAsync(seniorId);
        return Ok(result);
    }

    [HttpDelete("my-assignments/{assignmentId}")]
    public async Task<IActionResult> DeactivateMyAssignment(int assignmentId)
    {
        await _couponService.DeactivateAssignmentAsync(assignmentId);
        return NoContent();
    }

    [HttpPost("validate")]
    public async Task<ActionResult<CouponValidationResultDto>> Validate(
        [FromQuery] string code,
        [FromQuery] int seniorId,
        [FromQuery] decimal orderTotal)
    {
        var result = await _couponService.ValidateCodeForOrderAsync(code, seniorId, orderTotal);
        return Ok(result);
    }

    [HttpPost("apply")]
    public async Task<ActionResult<CouponValidationResultDto>> Apply(
        [FromQuery] string code,
        [FromQuery] int orderId,
        [FromQuery] int seniorId,
        [FromQuery] decimal orderTotal)
    {
        var result = await _couponService.ApplyToOrderAsync(code, orderId, seniorId, orderTotal);
        return Ok(result);
    }

    #endregion
}
