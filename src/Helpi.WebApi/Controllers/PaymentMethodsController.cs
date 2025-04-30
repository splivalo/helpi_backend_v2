
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

        public PaymentMethodsController(PaymentMethodService service) => _service = service;

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
        [HttpPost] public async Task<ActionResult<PaymentMethodDto>> Create(PaymentMethodCreateDto dto) => CreatedAtAction(nameof(GetByCustomer), new { }, await _service.AddPaymentMethodAsync(dto));
}