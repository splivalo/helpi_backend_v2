
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/student-services")]
public class StudentServicesController : ControllerBase
{
        private readonly StudentServiceService _service;

        public StudentServicesController(StudentServiceService service) => _service = service;

        [HttpPost] public async Task<IActionResult> Add(StudentServiceCreateDto dto) { await _service.AddServiceToStudentAsync(dto); return NoContent(); }
        [HttpDelete("{studentId}/{serviceId}")] public async Task<IActionResult> Remove(int studentId, int serviceId) { await _service.RemoveServiceFromStudentAsync(studentId, serviceId); return NoContent(); }
}