
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.JobInstance;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/job-instances")]
public class JobInstancesController : ControllerBase
{
        private readonly IJobInstanceService _jobInstanceService;

        public JobInstancesController(IJobInstanceService service) => _jobInstanceService = service;

        [HttpGet("assignment/{assignmentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetByAssignment(int assignmentId)
        {

                return
                 Ok(await _jobInstanceService.GetJobInstancesByAssignmentAsync(assignmentId));
        }
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetJobInstancesByStudent(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetJobInstancesByStudentAsync(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/senior/{seniorId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _jobInstanceService.GetSeniorCompletedJobInstances(seniorId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentCompletedJobInstances(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("upcoming/student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceService.GetStudentUpComingJobInstances(studentId);
                return Ok(jobInstances);
        }

        [HttpGet]
        public async Task<ActionResult<List<JobInstanceDto>>> GetJobInstances()
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
            [FromBody] ManageJobInstanceRequestDto request)
        {
                try
                {
                        var result = await _jobInstanceService.ManageJobInstance(
                            jobInstanceId,
                            request.NewDate,
                            request.NewStartTime,
                            request.NewEndTime,
                            request.Reason,
                            request.PreferedStudentId,
                            request.ReassignStudent,
                            request.RequestedByUserId);

                        if (result == null)
                                return BadRequest(new { message = "No changes were made to the job instance." });

                        return Ok(result);
                }
                catch (ArgumentException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                        // Log exception
                        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
                }
        }
}