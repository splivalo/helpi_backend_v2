
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
        private readonly InvoiceService _service;

        public InvoicesController(InvoiceService service) => _service = service;

        [HttpGet("customer/{customerId}")] public async Task<ActionResult<List<InvoiceDto>>> GetByCustomer(int customerId) => Ok(await _service.GetInvoicesByCustomerAsync(customerId));
        // [HttpPost] public async Task<ActionResult<InvoiceDto>> Create(InvoiceCreateDto dto) => CreatedAtAction(nameof(GetByCustomer), new { customerId = dto.CustomerId }, await _service.CreateInvoiceAsync(dto));
}
