
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/service-regions")]
public class ServiceRegionsController : ControllerBase
{
        private readonly ServiceRegionService _service;

        public ServiceRegionsController(ServiceRegionService service) => _service = service;

        [HttpGet("service/{serviceId}")] public async Task<ActionResult<List<ServiceRegionDto>>> GetByService(int serviceId) => Ok(await _service.GetRegionsByServiceAsync(serviceId));
        [HttpPost] public async Task<ActionResult<ServiceRegionDto>> Create(ServiceRegionCreateDto dto) => CreatedAtAction(nameof(GetByService), new { serviceId = dto.ServiceId }, await _service.CreateServiceRegionAsync(dto));
}