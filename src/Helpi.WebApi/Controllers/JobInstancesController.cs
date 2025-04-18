
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/job-instances")]
public class JobInstancesController : ControllerBase
{
        private readonly JobInstanceService _service;

        public JobInstancesController(JobInstanceService service) => _service = service;

        [HttpGet("assignment/{assignmentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetByAssignment(int assignmentId)
        {

                return
                 Ok(await _service.GetJobInstancesByAssignmentAsync(assignmentId));
        }
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetJobInstancesByStudent(int studentId)
        {
                var jobInstances = await _service.GetJobInstancesByStudentAsync(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/senior/{seniorId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _service.GetSeniorCompletedJobInstances(seniorId);
                return Ok(jobInstances);
        }
        [HttpGet("completed/student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _service.GetStudentCompletedJobInstances(studentId);
                return Ok(jobInstances);
        }
        [HttpGet("upcoming/student/{studentId}")]
        public async Task<ActionResult<List<JobInstanceDto>>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _service.GetStudentUpComingJobInstances(studentId);
                return Ok(jobInstances);
        }

        [HttpGet]
        public async Task<ActionResult<List<JobInstanceDto>>> GetJobInstances()
        {
                var jobInstances = await _service.GetJobInstances();
                return Ok(jobInstances);
        }
        // [HttpPost] public async Task<ActionResult<JobInstanceDto>> Create(JobInstanceCreateDto dto) => CreatedAtAction(nameof(GetByAssignment), new { assignmentId = dto.AssignmentId }, await _service.CreateJobInstanceAsync(dto));
}