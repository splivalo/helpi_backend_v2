
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[ApiController]
[Route("api/faculties")]
public class FacultiesController : ControllerBase
{
        private readonly FacultyService _service;

        public FacultiesController(FacultyService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<FacultyDto>>> GetAll() => Ok(await _service.GetAllFacultiesAsync());
        [HttpPost] public async Task<ActionResult<FacultyDto>> Create(FacultyCreateDto dto) => CreatedAtAction(nameof(GetAll), await _service.CreateFacultyAsync(dto));
}
