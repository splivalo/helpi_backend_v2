
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/student-contracts")]
public class StudentContractsController : ControllerBase
{
        private readonly StudentContractService _service;

        public StudentContractsController(StudentContractService service) => _service = service;

        [HttpGet("student/{studentId}")] public async Task<ActionResult<List<StudentContractDto>>> GetByStudent(int studentId) => Ok(await _service.GetContractsByStudentAsync(studentId));
        // [HttpPost] public async Task<ActionResult<StudentContractDto>> Create(StudentContractCreateDto dto) => CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, await _service.CreateContractAsync(dto));
}