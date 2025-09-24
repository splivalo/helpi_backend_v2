
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.Exceptions;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[Authorize]
[ApiController]
[Route("api/job-requests")]
public class JobRequestsController : ControllerBase
{
        private readonly JobRequestService _jobRequestService;

        public JobRequestsController(JobRequestService jobRequestService) => _jobRequestService = jobRequestService;


        [HttpPost]
        public async Task<ActionResult<JobRequestDto>> Create(JobRequestCreateDto dto)
        {

                var jobRequest = await _jobRequestService.CreateJobRequestAsync(dto);
                return CreatedAtAction(nameof(GetStudentPendingRequests), new { id = dto.StudentId }, jobRequest);
        }

        [HttpGet("pending/student/{studentId}")]
        public async Task<ActionResult<List<JobRequestDto>>> GetStudentPendingRequests(int studentId)
        {
                var requests = await _jobRequestService.GetStudentPendingRequests(studentId);
                return Ok(requests);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<JobRequestDto>>> GetStudentRequests(int studentId)
        {
                var requests = await _jobRequestService.GetStudentRequests(studentId);
                return Ok(requests);
        }


        [HttpPut("respond")]
        public async Task<ActionResult<JobRequestDto>> RespondToJobRequest(
            [FromBody] RespondToJobRequestDto respondToJobRequestDto)
        {
                try
                {
                        var jobRequestDto = await _jobRequestService.RespondToJobRequestAsync(respondToJobRequestDto);

                        return Ok(jobRequestDto);

                }
                catch (NotFoundException ex)
                {
                        return NotFound(new { Error = ex.Message });
                }
        }


}