
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/payment-transactions")]
public class PaymentTransactionsController : ControllerBase
{
        private readonly PaymentTransactionService _service;

        public PaymentTransactionsController(PaymentTransactionService service) => _service = service;

        //         [HttpGet("customer/{customerId}")] public async Task<ActionResult<List<PaymentTransactionDto>>> GetByCustomer(int customerId) => Ok(await _service.GetTransactionsByCustomerAsync(customerId));
        //         [HttpPost] public async Task<ActionResult<PaymentTransactionDto>> Create(PaymentTransactionCreateDto dto) => CreatedAtAction(nameof(GetByCustomer), new { customerId = dto.CustomerId }, await _service.CreateTransactionAsync(dto));
}