
using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
        private readonly StudentsService _service;

        public StudentsController(StudentsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<StudentDto>>> GetStudents(
        [FromQuery] int? cityId = null,
        [FromQuery] string? searchText = null,
        [FromQuery] List<int>? serviceIds = null,
        [FromQuery] StudentStatus? status = null,
        [FromQuery] int? facultyId = null,
        [FromQuery] List<AvailabilityCriteria>? availabilityCriteria = null,
        [FromQuery] bool matchAllAvailability = true,
        [FromQuery] bool? hasAvailabilitySlots = null,
        [FromQuery] decimal? minAverageRating = null,
        [FromQuery] bool? backgroundCheckCompleted = null,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] bool excludeConflicts = false)
        {
                var filter = new StudentFilterDto
                {
                        CityId = cityId,
                        SearchText = searchText,
                        ServiceIds = serviceIds,
                        Status = status,
                        FacultyId = facultyId,
                        AvailabilityCriteria = availabilityCriteria,
                        MatchAllAvailability = matchAllAvailability,
                        HasAvailabilitySlots = hasAvailabilitySlots,
                        MinAverageRating = minAverageRating,
                        BackgroundCheckCompleted = backgroundCheckCompleted,
                        IncludeDeleted = includeDeleted,
                        ExcludeConflicts = excludeConflicts
                };

                var students = await _service.GetStudentsAsync(filter);
                return Ok(students);
        }



        [HttpGet("order-schedules/{orderScheduleId}/available-students")]
        public async Task<ActionResult<List<StudentDto>>> GetAvailableStudentsForOrderSchedule(int orderScheduleId)
        {
                var students = await _service.FindEligibleStudentsForSchedule(orderScheduleId, null);
                return Ok(students);
        }


        [HttpGet("available-students")]
        public async Task<ActionResult<List<StudentDto>>> GetAvailableStudents(DateOnly date,
                                TimeOnly startTime,
                                TimeOnly endTime,
                                int orderId,
 [FromQuery] List<int>? excludeJobInstanceIds = null
                                )
        {
                var students = await _service.FindEligibleStudentsForInstance2(date,
                                 startTime,
                                 endTime,
                                 orderId,
                                 excludeJobInstanceIds ?? new List<int>());
                return Ok(students);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetById(int id)
        {

                var student = await _service.GetStudentByIdAsync(id);
                return Ok(student);
        }
        [HttpPost]
        public async Task<ActionResult<StudentDto>> Create(StudentCreateDto dto)
        {

                var student = await _service.CreateStudentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = student.UserId }, dto);
        }

        [HttpPatch("{id}/verification")]
        public async Task<IActionResult> UpdateVerification(int id, [FromBody] StudentStatus status) { await _service.UpdateVerificationStatusAsync(id, status); return NoContent(); }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentDto>> Update(int id, StudentUpdateDto dto)
        {
                var student = await _service.UpdateStudentAsync(id, dto);
                return Ok(student);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
                // Get current user info from JWT claims
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
                {
                        return Unauthorized(new { message = "Invalid user identity" });
                }

                var isAdmin = User.IsInRole(UserType.Admin.ToString());
                var isStudent = User.IsInRole(UserType.Student.ToString());

                // Authorization: Admin can delete any student, Student can only delete their own account
                if (!isAdmin && !(isStudent && currentUserId == id))
                {
                        return Forbid();
                }

                var result = await _service.PermanentlyDeleteStudent(id);
                if (result)
                {
                        return NoContent();
                }

                return StatusCode(500, new { message = "Failed to delete student" });
        }

        /// <summary>
        /// Check if student can be archived and get blocking item counts.
        /// </summary>
        [HttpGet("{id}/archive-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveCheckDto>> GetArchiveCheck(int id)
        {
                var check = await _service.GetArchiveCheckAsync(id);
                return Ok(check);
        }

        /// <summary>
        /// Archive a student. If force=true, terminates all assignments and cancels sessions.
        /// </summary>
        [HttpPost("{id}/archive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> ArchiveStudent(int id, [FromBody] ArchiveRequestDto request)
        {
                var result = await _service.ArchiveStudentAsync(id, request);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }

        /// <summary>
        /// Unarchive a student.
        /// </summary>
        [HttpPost("{id}/unarchive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> UnarchiveStudent(int id)
        {
                var result = await _service.UnarchiveStudentAsync(id);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }
}