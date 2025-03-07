
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/job-instances")]
public class JobInstancesController : ControllerBase
{
        private readonly JobInstanceService _service;

        public JobInstancesController(JobInstanceService service) => _service = service;

        [HttpGet("assignment/{assignmentId}")] public async Task<ActionResult<List<JobInstanceDto>>> GetByAssignment(int assignmentId) => Ok(await _service.GetJobInstancesByAssignmentAsync(assignmentId));
        // [HttpPost] public async Task<ActionResult<JobInstanceDto>> Create(JobInstanceCreateDto dto) => CreatedAtAction(nameof(GetByAssignment), new { assignmentId = dto.AssignmentId }, await _service.CreateJobInstanceAsync(dto));
}