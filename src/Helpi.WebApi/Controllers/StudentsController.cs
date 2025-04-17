
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
        private readonly StudentService _service;

        public StudentsController(StudentService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<StudentDto>>> GetAll()
        {
                var students = await _service.GetAllStudentsAsync();
                return Ok(students);
        }



        [HttpGet("order-schedules/{orderScheduleId}/available-students")]
        public async Task<ActionResult<List<StudentDto>>> GetAvailableStudentsForOrderSchedule(int orderScheduleId)
        {
                var students = await _service.GetAvailableStudentsForOrderSchedule(orderScheduleId);
                return Ok(students);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetById(int id)
        {

                var student = await _service.GetStudentByIdAsync(id);
                return Ok(student);
        }
        [HttpPost]
        public async Task<ActionResult<StudentDto>> Create(StudentCreateDto dto)
        {

                var student = await _service.CreateStudentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = student.UserId }, dto);
        }

        [HttpPatch("{id}/verification")]
        public async Task<IActionResult> UpdateVerification(int id, [FromBody] VerificationStatus status) { await _service.UpdateVerificationStatusAsync(id, status); return NoContent(); }
}