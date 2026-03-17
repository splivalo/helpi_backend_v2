
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
        public async Task<ActionResult<StudentContractDto>> Create([FromForm] StudentContractCreateDto dto)
        {
                var contractDto = await _service.CreateContractAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, contractDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentContractDto>> Update(int id, [FromForm] StudentContractUpdateDto dto)
        {
                var contractDto = await _service.UpdateContractAsync(id, dto);
                return CreatedAtAction(nameof(GetByStudent), new { studentId = contractDto.StudentId }, contractDto);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudentContract(int id)
        {
                await _service.DeleteContractAsync(id);


                return NoContent(); // 204
        }

        [HttpGet("completed/{studentId}")]
        public async Task<IActionResult> GetCompletedContracts(int studentId)
        {
                var contracts = await _service.GetStudentCompletedContractsAsync(studentId);
                return Ok(contracts);
        }

        /// <summary>
        /// Check if contract can be deleted and get blocking item counts.
        /// </summary>
        [HttpGet("{id}/delete-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveCheckDto>> GetDeleteCheck(int id)
        {
                var check = await _service.GetDeleteCheckAsync(id);
                return Ok(check);
        }

        /// <summary>
        /// Delete a contract with check. If force=true, reassigns all sessions first.
        /// </summary>
        [HttpDelete("{id}/with-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> DeleteWithCheck(int id, [FromBody] ArchiveRequestDto request)
        {
                var result = await _service.DeleteContractWithCheckAsync(id, request);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }

}