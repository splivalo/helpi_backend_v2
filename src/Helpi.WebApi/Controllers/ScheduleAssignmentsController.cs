
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/schedule-assignments")]
public class ScheduleAssignmentsController : ControllerBase
{
        private readonly ScheduleAssignmentService _service;

        public ScheduleAssignmentsController(ScheduleAssignmentService service) => _service = service;

        [HttpGet("student/{studentId}")] public async Task<ActionResult<List<ScheduleAssignmentDto>>> GetByStudent(int studentId) => Ok(await _service.GetAssignmentsByStudentAsync(studentId));

        [HttpGet("order-schedule/{orderScheduleId}/active-student")]
        public async Task<ActionResult<StudentDto>> GetActiveStudentForOrderScheduleAsync(int orderScheduleId)
        {
                var student = await _service.GetActiveStudentForOrderScheduleAsync(orderScheduleId);
                return Ok(student);
        }
        [HttpPost] public async Task<ActionResult<ScheduleAssignmentDto>> Create(ScheduleAssignmentCreateDto dto) => CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, await _service.CreateAssignmentAsync(dto));
}