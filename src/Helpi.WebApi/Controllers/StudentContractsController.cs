
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/student-contracts")]
public class StudentContractsController : ControllerBase
{
        private readonly StudentContractService _service;

        public StudentContractsController(StudentContractService service) => _service = service;

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<StudentContractDto>>> GetByStudent(int studentId)
        {

                var contracts = await _service.GetContractsByStudentAsync(studentId);
                return Ok(contracts);
        }


        [HttpPost]
        public async Task<ActionResult<StudentContractDto>> Create(StudentContractCreateDto dto)
        {
                var contractDto = await _service.CreateContractAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, contractDto);
        }

        [HttpPut("id")]
        public async Task<ActionResult<StudentContractDto>> Update(int id, StudentContractUpdateDto dto)
        {
                var contractDto = await _service.UpdateContractAsync(id, dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, contractDto);
        }

        [HttpGet("completed/{studentId}")]
        public async Task<IActionResult> GetCompletedContracts(int studentId)
        {
                var contracts = await _service.GetStudentCompletedContractsAsync(studentId);
                return Ok(contracts);
        }
}