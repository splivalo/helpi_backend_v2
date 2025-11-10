
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
        [FromQuery] bool includeDeleted = false)
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
                        IncludeDeleted = includeDeleted
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
}