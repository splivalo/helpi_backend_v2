
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/service-categories")]
public class ServiceCategoriesController : ControllerBase
{
        private readonly ServiceCategoryService _service;

        public ServiceCategoriesController(ServiceCategoryService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<ServiceCategoryDto>>> GetAll() => Ok(await _service.GetAllCategoriesAsync());
        [HttpPost] public async Task<ActionResult<ServiceCategoryDto>> Create(ServiceCategoryCreateDto dto) => CreatedAtAction(nameof(GetAll), await _service.CreateCategoryAsync(dto));
}