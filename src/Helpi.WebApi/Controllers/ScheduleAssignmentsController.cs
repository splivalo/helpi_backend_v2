
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Helpi.Application.DTOs.ScheduleAssignments;

namespace Helpi.WebApi.Controllers;

[Authorize]
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

        [HttpGet("order-schedule/{orderScheduleId}/assignment")]
        public async Task<ActionResult<ScheduleAssignmentDto>> GetAssignmentForOrderScheduleAsync(int orderScheduleId)
        {
                var ass = await _service.GetAssignmentForOrderScheduleAsync(orderScheduleId);
                return Ok(ass);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost] public async Task<ActionResult<ScheduleAssignmentDto>> Create(ScheduleAssignmentCreateDto dto) => CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, await _service.CreateAssignmentAsync(dto));

        [Authorize(Roles = "Admin")]
        [HttpPost("admin-assign")]
        public async Task<ActionResult<ScheduleAssignmentDto>> AdminDirectAssign([FromBody] ScheduleAssignmentCreateDto dto)
        {
                var result = await _service.AdminDirectAssignAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, result);
        }

        [HttpPost("reassign")]
        public async Task<IActionResult> ReassignScheduleAssignment([FromBody] ReassignScheduleAssignmentRequestDto request)
        {
                if (request == null || request.ScheduleAssignmentId <= 0)
                        return BadRequest("Invalid request");




                (bool success, string message) = await _service.ReassignScheduleAssignment(
                   request.ScheduleAssignmentId,
                   request.PreferedStudentId
               );

                if (!success)
                        return StatusCode(StatusCodes.Status500InternalServerError, new { success, message });

                return Ok(new { message });
        }
}