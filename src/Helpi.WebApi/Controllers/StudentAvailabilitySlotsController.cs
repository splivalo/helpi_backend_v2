
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/student-availability-slots")]
public class StudentAvailabilitySlotsController : ControllerBase
{
        private readonly StudentAvailabilitySlotService _service;

        public StudentAvailabilitySlotsController(StudentAvailabilitySlotService service) => _service = service;

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<StudentAvailabilitySlotDto>>> GetByStudent(int studentId)
        {
                return Ok(await _service.GetSlotsByStudentAsync(studentId));
        }

        public async Task<ActionResult<StudentAvailabilitySlotDto>> GetByIdAsync(int studentId, byte dayOfWeek)
        {
                var slot = await _service.GetByIdAsync(studentId, dayOfWeek);
                return Ok(slot);
        }



        /// <summary>
        /// /api/student-slots/101/day/1/range?start=09:00
        /// </summary>
        [HttpGet("{studentId}/day/{day}/range")]
        public async Task<ActionResult<IEnumerable<StudentAvailabilitySlotDto>>> GetByDayAndTimeRange(
               int studentId, DayOfWeek day, [FromQuery] TimeOnly start, [FromQuery] TimeOnly end)
        {
                var slots = await _service.GetSlotsByDayAndTimeRangeAsync(studentId, day, start, end);
                return Ok(slots);
        }
        [HttpPost]
        public async Task<ActionResult<StudentAvailabilitySlotDto>> Create(StudentAvailabilitySlotCreateDto dto)
        {
                var slot = await _service.CreateSlotAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, slot);
        }

        [HttpPut("{studentId}/{dayOfWeek}")]
        public async Task<ActionResult<StudentAvailabilitySlotDto>> Update(int studentId, byte dayOfWeek, [FromBody] StudentAvailabilitySlotUpdateDto dto)
        {
                var updatedSlot = await _service.UpdateSlotAsync(studentId, dayOfWeek, dto);
                return Ok(updatedSlot);
        }

        [HttpDelete("{studentId}/{dayOfWeek}")]
        public async Task<IActionResult> Delete(int studentId, byte dayOfWeek)
        {
                await _service.DeleteSlotAsync(studentId, dayOfWeek);
                return NoContent(); // 204 No Content
        }
}