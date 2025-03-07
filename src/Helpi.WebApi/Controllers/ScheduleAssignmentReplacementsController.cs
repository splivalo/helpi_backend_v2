
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/schedule-assignment-replacements")]
public class ScheduleAssignmentReplacementsController : ControllerBase
{
        private readonly ScheduleAssignmentReplacementService _service;

        public ScheduleAssignmentReplacementsController(ScheduleAssignmentReplacementService service) => _service = service;

        // [HttpGet("original/{originalId}")] public async Task<ActionResult<List<ScheduleAssignmentReplacementDto>>> GetByOriginal(int originalId) => Ok(await _service.GetReplacementsByOriginalAsync(originalId));
        [HttpPost]
        public async Task<ActionResult<ScheduleAssignmentReplacementDto>> Create(ScheduleAssignmentReplacementCreateDto dto)
        {
                return null;

                //   CreatedAtAction(nameof(GetByOriginal), new { originalId = dto.OriginalAssignmentId }, await _service.CreateReplacementAsync(dto));
        }
}