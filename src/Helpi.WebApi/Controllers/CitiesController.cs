
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/cities")]
public class CitiesController : ControllerBase
{
        private readonly CityService _service;

        public CitiesController(CityService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<CityDto>>> GetAll() => Ok(await _service.GetAllCitiesAsync());
        [HttpPost] public async Task<ActionResult<CityDto>> Create(CityCreateDto dto) => CreatedAtAction(nameof(GetAll), await _service.CreateCityAsync(dto));
}