
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.JobInstance;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
        private readonly IJobInstanceService _jobInstanceService;

        public SessionsController(IJobInstanceService service) => _jobInstanceService = service;

        [HttpGet("assignment/{assignmentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetByAssignment(int assignmentId)
        {

                return
                 Ok(await _jobInstanceService.GetJobInstancesByAssignmentAsync(assignmentId));
        }
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<SessionDto>>> GetByOrder(int orderId)
        {
                return Ok(await _jobInstanceService.GetJobInstancesByOrderAsync(orderId));
        }
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetJobInstancesByStudent(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetJobInstancesByStudentAsync(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/senior/{seniorId}")]
        public async Task<ActionResult<List<SessionDto>>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _jobInstanceService.GetSeniorCompletedJobInstances(seniorId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentCompletedJobInstances(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("upcoming/student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentUpComingJobInstances(studentId);
                return Ok(jobInstances);
        }

        [HttpGet]
        public async Task<ActionResult<List<SessionDto>>> GetJobInstances()
        {
                var jobInstances = await _jobInstanceService.GetJobInstances();
                return Ok(jobInstances);
        }
        // [HttpPost] public async Task<ActionResult<JobInstanceDto>> Create(JobInstanceCreateDto dto) => CreatedAtAction(nameof(GetByAssignment), new { assignmentId = dto.AssignmentId }, await _service.CreateJobInstanceAsync(dto));

        /// <summary>
        /// Manage a job instance (reschedule and/or change assigned student).
        /// </summary>
        [HttpPost("{jobInstanceId}/manage")]
        public async Task<IActionResult> ManageJobInstance(
            int jobInstanceId,
            [FromBody] ManageSessionRequestDto request)
        {
                try
                {
                        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!int.TryParse(userIdString, out var requestedByUserId))
                                return Unauthorized(new { message = "Invalid user token." });

                        var result = await _jobInstanceService.ManageJobInstance(
                            jobInstanceId,
                            request.NewDate,
                            request.NewStartTime,
                            request.NewEndTime,
                            request.Reason,
                            request.PreferredStudentId,
                            request.ReassignStudent,
                            requestedByUserId);

                        if (result == null)
                                return BadRequest(new { message = "No changes were made to the job instance." });

                        return Ok(result);
                }
                catch (ArgumentException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                        // Log exception
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }

        [HttpPost("{jobInstanceId}/cancel")]
        public async Task<ActionResult<SessionDto>> CancelJobInstance(
           int jobInstanceId)
        {
                try
                {
                        var result = await _jobInstanceService.CancelJobInstance(
                            jobInstanceId);

                        if (result == null)
                                return BadRequest(new { message = "No changes were made to the job instance." });

                        return Ok(result);
                }
                catch (Exception ex)
                {
                        // Log exception
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }

        [HttpPost("{jobInstanceId}/reactivate")]
        public async Task<ActionResult<SessionDto>> ReactivateJobInstance(
           int jobInstanceId)
        {
                try
                {
                        var result = await _jobInstanceService.ReactivateJobInstance(
                            jobInstanceId);

                        if (result == null)
                                return BadRequest(new { message = "No changes were made to the job instance." });

                        return Ok(result);
                }
                catch (Exception ex)
                {
                        // Log exception
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }
}