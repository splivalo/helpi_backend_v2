
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/seniors")]
public class SeniorsController : ControllerBase
{
        private readonly SeniorService _service;

        public SeniorsController(SeniorService service) => _service = service;

        [HttpGet("customer/{customerId}")] public async Task<ActionResult<List<SeniorDto>>> GetByCustomer(int customerId) => Ok(await _service.GetSeniorsByCustomerAsync(customerId));
        [HttpPost] public async Task<ActionResult<SeniorDto>> Create(SeniorCreateDto dto) => CreatedAtAction(nameof(GetByCustomer), new { customerId = dto.CustomerId }, await _service.CreateSeniorAsync(dto));
}