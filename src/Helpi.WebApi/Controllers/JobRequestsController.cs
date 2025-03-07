
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/job-requests")]
public class JobRequestsController : ControllerBase
{
        private readonly JobRequestService _service;

        public JobRequestsController(JobRequestService service) => _service = service;

        [HttpGet("pending")] public async Task<ActionResult<List<JobRequestDto>>> GetPending() => Ok(await _service.GetPendingRequestsAsync());
        [HttpPost] public async Task<ActionResult<JobRequestDto>> Create(JobRequestCreateDto dto) => CreatedAtAction(nameof(GetPending), await _service.CreateJobRequestAsync(dto));
}