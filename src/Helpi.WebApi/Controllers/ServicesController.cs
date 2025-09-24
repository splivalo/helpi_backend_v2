
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
        private readonly ServiceService _service;

        public ServicesController(ServiceService service) => _service = service;

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<List<ServiceDto>>> GetByCategory(int categoryId)
        {
                return Ok(await _service.GetServicesByCategoryAsync(categoryId));
        }
        [HttpPost]
        public async Task<ActionResult<ServiceDto>> Create(ServiceCreateDto dto)
        {

                var service = await _service.CreateServiceAsync(dto);
                return CreatedAtAction(nameof(GetByCategory), new { categoryId = dto.CategoryId }, service);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceCategoryDto>> Update(int id, ServiceUpdateDto dto)
        {
                var updated = await _service.UpdateServiceAsync(id, dto);

                if (updated == null)
                        return NotFound();

                return Ok(updated);
        }
}