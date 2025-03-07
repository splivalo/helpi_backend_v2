
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
        private readonly StudentService _service;

        public StudentsController(StudentService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<StudentDto>>> GetAll() => Ok(await _service.GetAllStudentsAsync());
        [HttpGet("{id}")] public async Task<ActionResult<StudentDto>> GetById(int id) => Ok(await _service.GetStudentByIdAsync(id));
        [HttpPost] public async Task<ActionResult<StudentDto>> Create(StudentCreateDto dto) => CreatedAtAction(nameof(GetById), new { id = (await _service.CreateStudentAsync(dto)).Id }, dto);
        [HttpPatch("{id}/verification")] public async Task<IActionResult> UpdateVerification(int id, [FromBody] VerificationStatus status) { await _service.UpdateVerificationStatusAsync(id, status); return NoContent(); }
}