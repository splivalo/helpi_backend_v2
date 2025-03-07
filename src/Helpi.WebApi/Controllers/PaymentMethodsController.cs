
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
        private readonly PaymentMethodService _service;

        public PaymentMethodsController(PaymentMethodService service) => _service = service;

        [HttpGet("customer/{customerId}")] public async Task<ActionResult<List<PaymentMethodDto>>> GetByCustomer(int customerId) => Ok(await _service.GetMethodsByCustomerAsync(customerId));
        [HttpPost] public async Task<ActionResult<PaymentMethodDto>> Create(PaymentMethodCreateDto dto) => CreatedAtAction(nameof(GetByCustomer), new { }, await _service.AddPaymentMethodAsync(dto));
}