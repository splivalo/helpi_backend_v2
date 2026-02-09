using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
        private readonly PaymentMethodService _service;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(PaymentMethodService service, ILogger<PaymentMethodsController> logger)
        {
                _service = service;
                _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<PaymentMethodDto>>> GetByCustomer(int userId)
        {
                var pms = await _service.GetMethodsByUserIdAsync(userId);
                return Ok(pms);
        }

        [HttpPost("sync-stripe/user/{userId}")]
        public async Task SycWithStripeForUser(int userId)
        {
                await _service.SyncAllPaymentMethodsWithStripeForCustomerAsync(userId);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> Create(PaymentMethodCreateDto dto) =>
                CreatedAtAction(nameof(GetByCustomer), new { }, await _service.AddPaymentMethodAsync(dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
                try
                {
                        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(userIdString))
                        {
                                return Unauthorized();
                        }

                        var userId = int.Parse(userIdString);
                        await _service.DeletePaymentMethodAsync(id, userId);

                        return NoContent();
                }
                catch (KeyNotFoundException)
                {
                        return NotFound("Payment method not found");
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error deleting payment method {PaymentMethodId}", id);
                        return StatusCode(500, "An error occurred while deleting the payment method");
                }
        }
}