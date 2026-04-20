
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Helpi.Application.DTOs.ScheduleAssignments;
using System.Security.Claims;

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
        [HttpGet("admin-pending")]
        public async Task<IActionResult> GetAllPendingForAdmin()
        {
                var result = await _service.GetAllPendingAcceptanceForAdminAsync();
                return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin-assign")]
        public async Task<ActionResult<ScheduleAssignmentDto>> AdminDirectAssign([FromBody] ScheduleAssignmentCreateDto dto)
        {
                var result = await _service.AdminDirectAssignAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin-terminate/{orderScheduleId}")]
        public async Task<IActionResult> AdminTerminateAssignment(int orderScheduleId)
        {
                await _service.AdminTerminateAssignmentAsync(orderScheduleId);
                return Ok(new { message = "Assignment terminated" });
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

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/accept")]
        public async Task<ActionResult<ScheduleAssignmentDto>> AcceptAssignment(int id)
        {
                try
                {
                        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                        var result = await _service.AcceptAssignmentAsync(id, userId);
                        return Ok(result);
                }
                catch (DomainException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineAssignment(int id)
        {
                try
                {
                        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                        await _service.DeclineAssignmentAsync(id, userId);
                        return Ok(new { message = "Assignment declined" });
                }
                catch (DomainException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
        }

        [Authorize(Roles = "Student")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAssignments()
        {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetPendingAssignmentsByStudentUserIdAsync(userId);
                return Ok(result);
        }
}