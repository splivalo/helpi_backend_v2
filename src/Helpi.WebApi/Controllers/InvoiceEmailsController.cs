
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/invoice-emails")]
public class InvoiceEmailsController : ControllerBase
{
        private readonly InvoiceEmailService _service;

        public InvoiceEmailsController(InvoiceEmailService service) => _service = service;

        [HttpGet("invoice/{invoiceId}")] public async Task<ActionResult<List<InvoiceEmailDto>>> GetByInvoice(int invoiceId) => Ok(await _service.GetEmailsByInvoiceAsync(invoiceId));
        // [HttpPost] public async Task<ActionResult<InvoiceEmailDto>> Create(InvoiceEmailCreateDto dto) => CreatedAtAction(nameof(GetByInvoice), new { invoiceId = dto.InvoiceId }, await _service.CreateInvoiceEmailAsync(dto));
}