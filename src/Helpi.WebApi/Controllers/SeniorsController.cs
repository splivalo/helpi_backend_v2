
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/seniors")]
public class SeniorsController : ControllerBase
{
        private readonly SeniorService _service;

        public SeniorsController(SeniorService service) => _service = service;


        [HttpGet]
        public async Task<ActionResult<List<SeniorDto>>> GetBySeniors()
        {
                var senior = await _service.GetBySeniorsAsync();
                return Ok(senior);
        }


        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<SeniorDto>>> GetByCustomer(int customerId)
        {
                var seniors = await _service.GetSeniorsByCustomerAsync(customerId);
                return Ok(seniors);
        }
        [HttpPost]
        public async Task<ActionResult<SeniorDto>> Create(SeniorCreateDto dto)
        {
                var s = await _service.CreateSeniorAsync(dto);
                return CreatedAtAction(nameof(GetByCustomer), new { customerId = dto.CustomerId }, s);
        }
}