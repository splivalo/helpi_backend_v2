
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/student-services")]
public class StudentServicesController : ControllerBase
{
        private readonly StudentServiceService _service;

        public StudentServicesController(StudentServiceService service) => _service = service;

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<StudentServiceDto>>> GetByStudent(int studentId)
        {
                var studentServices = await _service.GetByStudentAsync(studentId);
                return Ok(studentServices);
        }

        [HttpPost]
        public async Task<ActionResult<StudentServiceDto>> Add(StudentServiceCreateDto dto)
        {
                var studentService = await _service.AddServiceToStudentAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, studentService);

        }

        [HttpDelete("student/{studentId}/service/{serviceId}")]
        public async Task<IActionResult> Remove(int studentId, int serviceId)
        {
                await _service.RemoveServiceFromStudentAsync(studentId, serviceId);
                return NoContent();
        }
}