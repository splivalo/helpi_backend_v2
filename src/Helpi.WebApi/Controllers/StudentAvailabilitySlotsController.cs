
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/student-availability-slots")]
public class StudentAvailabilitySlotsController : ControllerBase
{
        private readonly StudentAvailabilitySlotService _service;

        public StudentAvailabilitySlotsController(StudentAvailabilitySlotService service) => _service = service;

        [HttpGet("student/{studentId}")] public async Task<ActionResult<List<StudentAvailabilitySlotDto>>> GetByStudent(int studentId) => Ok(await _service.GetSlotsByStudentAsync(studentId));
        [HttpPost] public async Task<ActionResult<StudentAvailabilitySlotDto>> Create(StudentAvailabilitySlotCreateDto dto) => CreatedAtAction(nameof(GetByStudent), new { }, await _service.CreateSlotAsync(dto));
}