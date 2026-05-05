
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

        private string CallerRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

        [HttpGet("assignment/{assignmentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetByAssignment(int assignmentId)
        {
                var sessions = await _jobInstanceService.GetJobInstancesByAssignmentAsync(assignmentId);
                await _jobInstanceService.StampCanCancelAsync(sessions, CallerRole);
                return Ok(sessions);
        }
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<SessionDto>>> GetByOrder(
            int orderId,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null)
        {
                var sessions = await _jobInstanceService.GetJobInstancesByOrderAsync(orderId, from, to);
                await _jobInstanceService.StampCanCancelAsync(sessions, CallerRole);
                return Ok(sessions);
        }
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetJobInstancesByStudent(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetJobInstancesByStudentAsync(studentId);
                await _jobInstanceService.StampCanCancelAsync(jobInstances, CallerRole);
                return Ok(jobInstances);
        }
        [HttpGet("completed/senior/{seniorId}")]
        public async Task<ActionResult<List<SessionDto>>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _jobInstanceService.GetSeniorCompletedJobInstances(seniorId);
                await _jobInstanceService.StampCanCancelAsync(jobInstances, CallerRole);
                return Ok(jobInstances);
        }
        [HttpGet("completed/student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentCompletedJobInstances(studentId);
                await _jobInstanceService.StampCanCancelAsync(jobInstances, CallerRole);
                return Ok(jobInstances);
        }
        [HttpGet("upcoming/student/{studentId}")]
        public async Task<ActionResult<List<SessionDto>>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentUpComingJobInstances(studentId);
                await _jobInstanceService.StampCanCancelAsync(jobInstances, CallerRole);
                return Ok(jobInstances);
        }

        [HttpGet]
        public async Task<ActionResult<List<SessionDto>>> GetJobInstances()
        {
                var jobInstances = await _jobInstanceService.GetJobInstances();
                await _jobInstanceService.StampCanCancelAsync(jobInstances, CallerRole);
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
                        var isAdmin = User.IsInRole("Admin");
                        var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
                        var result = await _jobInstanceService.CancelJobInstance(
                            jobInstanceId, isAdmin, callerRole);

                        if (result == null)
                                return BadRequest(new { message = "No changes were made to the job instance." });

                        return Ok(result);
                }
                catch (Helpi.Domain.Exceptions.DomainException ex)
                {
                        return BadRequest(new { message = ex.Message });
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

        /// <summary>
        /// Atomically reactivates a cancelled session with optional in-place date/time update
        /// and/or student change (creates PendingAcceptance assignment + notifies student).
        /// Use this instead of the two-call pattern (reactivate → manage).
        /// </summary>
        [HttpPost("{jobInstanceId}/reactivate-manage")]
        public async Task<ActionResult<SessionDto>> ReactivateAndManageJobInstance(
            int jobInstanceId,
            [FromBody] ReactivateAndManageRequestDto request)
        {
                try
                {
                        var result = await _jobInstanceService.ReactivateAndManageJobInstance(
                            jobInstanceId,
                            request.NewDate,
                            request.NewStartTime,
                            request.NewEndTime,
                            request.PreferredStudentId);

                        if (result == null)
                                return BadRequest(new { message = "Failed to reactivate session." });

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
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }

        /// <summary>
        /// Ensure a session is completed (idempotent). Used when frontend detects
        /// session time has passed but Hangfire hasn't completed it yet.
        /// Creates pending reviews if they don't exist.
        /// </summary>
        [HttpPost("{jobInstanceId}/ensure-completed")]
        public async Task<IActionResult> EnsureCompleted(int jobInstanceId)
        {
                try
                {
                        var success = await _jobInstanceService.EnsureCompletedAsync(jobInstanceId);
                        if (!success)
                                return BadRequest(new { message = "Session cannot be completed yet." });

                        return Ok(new { message = "Session completed and reviews created." });
                }
                catch (Exception ex)
                {
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }
}