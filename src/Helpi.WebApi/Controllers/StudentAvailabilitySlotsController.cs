
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Exceptions;
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

        [HttpGet("student/{studentId}/dayOfWeek/{dayOfWeek}")]
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

        [HttpPost("bulk")]
        public async Task<ActionResult<List<StudentAvailabilitySlotDto>>> CreateRange(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                if (dtos == null || dtos.Count == 0)
                {
                        return BadRequest("No slots provided.");
                }

                var slots = await _service.CreateSlotsAsync(dtos);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dtos.First().StudentId }, slots);
        }

        [HttpPut("bulk")]
        public async Task<ActionResult<List<StudentAvailabilitySlotDto>>> UpdateRange(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                try
                {
                        if (dtos == null || dtos.Count == 0)
                        {
                                return BadRequest("No slots provided.");
                        }

                        var isAdmin = User.IsInRole("Admin");
                        var slots = await _service.UpdateSlotsAsync(dtos, isAdmin);
                        return CreatedAtAction(nameof(GetByStudent), new { studentId = dtos.First().StudentId }, slots);
                }
                catch (DomainException ex)
                {
                        return BadRequest(new
                        {
                                Code = "CANCEL_CUTOFF",
                                Message = ex.Message,
                        });
                }
                catch (ActiveAssignmentException ex)
                {
                        return BadRequest(new
                        {
                                Code = "ACTIVE_ASSIGNMENTS_EXIST",
                                Message = ex.Message,
                        });
                }
                catch (Exception)
                {
                        return StatusCode(500, new
                        {
                                Code = "INTERNAL_SERVER_ERROR",
                                Message = "An unexpected error occurred."
                        });
                }
        }
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteRange(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                try
                {
                        if (dtos == null || dtos.Count == 0)
                        {
                                return BadRequest("No slots provided.");
                        }

                        await _service.DeleteSlotsAsync(dtos);
                        return NoContent();
                }
                catch (DomainException ex)
                {
                        return BadRequest(new
                        {
                                Code = "CANCEL_CUTOFF",
                                Message = ex.Message,
                        });
                }
                catch (ActiveAssignmentException ex)
                {
                        return BadRequest(new
                        {
                                Code = "ACTIVE_ASSIGNMENTS_EXIST",
                                Message = ex.Message,
                        });
                }
                catch (Exception)
                {
                        return StatusCode(500, new
                        {
                                Code = "INTERNAL_SERVER_ERROR",
                                Message = "An unexpected error occurred."
                        });
                }
        }

        [HttpPut("{studentId}/{dayOfWeek}")]
        public async Task<ActionResult<StudentAvailabilitySlotDto>> Update(int studentId, byte dayOfWeek, [FromBody] StudentAvailabilitySlotUpdateDto dto)
        {
                try
                {
                        var updatedSlot = await _service.UpdateSlotAsync(studentId, dayOfWeek, dto);
                        return Ok(updatedSlot);
                }
                catch (ActiveAssignmentException ex)
                {
                        return BadRequest(new
                        {
                                Code = "ACTIVE_ASSIGNMENTS_EXIST",
                                Message = ex.Message,
                        });
                }
                catch (Exception)
                {
                        return StatusCode(500, new
                        {
                                Code = "INTERNAL_SERVER_ERROR",
                                Message = "An unexpected error occurred."
                        });
                }
        }

        [HttpDelete("{studentId}/{dayOfWeek}")]
        public async Task<IActionResult> Delete(int studentId, byte dayOfWeek)
        {
                try
                {
                        await _service.DeleteSlotAsync(studentId, dayOfWeek);
                        return NoContent(); // 204 No Content
                }
                catch (ActiveAssignmentException ex)
                {
                        return BadRequest(new
                        {
                                Code = "ACTIVE_ASSIGNMENTS_EXIST",
                                Message = ex.Message,
                        });
                }
                catch (Exception)
                {
                        return StatusCode(500, new
                        {
                                Code = "INTERNAL_SERVER_ERROR",
                                Message = "An unexpected error occurred."
                        });
                }
        }
}