
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/service-categories")]
public class ServiceCategoriesController : ControllerBase
{
        private readonly ServiceCategoryService _service;

        public ServiceCategoriesController(ServiceCategoryService service) => _service = service;


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ServiceCategoryDto>>> GetAll()
        {
                return Ok(await _service.GetAllCategoriesAsync());
        }
        [HttpPost]
        public async Task<ActionResult<ServiceCategoryDto>> Create(ServiceCategoryCreateDto dto)
        {
                var category = await _service.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetAll), category);
        }
}