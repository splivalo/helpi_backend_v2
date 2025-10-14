
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/helpi-emails")]
public class HEmailsController : ControllerBase
{
        private readonly HEmailService _service;

        public HEmailsController(HEmailService service) => _service = service;

        [HttpGet("invoice/{invoiceId}")] public async Task<ActionResult<List<HEmailDto>>> GetByInvoice(int invoiceId) => Ok(await _service.GetEmailsByInvoiceAsync(invoiceId));
        // [HttpPost] public async Task<ActionResult<InvoiceEmailDto>> Create(InvoiceEmailCreateDto dto) => CreatedAtAction(nameof(GetByInvoice), new { invoiceId = dto.InvoiceId }, await _service.CreateInvoiceEmailAsync(dto));
}