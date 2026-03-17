
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/seniors")]
public class SeniorsController : ControllerBase
{
        private readonly SeniorService _service;

        public SeniorsController(SeniorService service) => _service = service;


        [HttpGet]
        public async Task<ActionResult<List<SeniorDto>>> GetSeniorsAsync(
                [FromQuery] int? cityId = null,
                [FromQuery] OrderStatus? orderStatus = null,
                [FromQuery] string? searchText = null
        )
        {

                var filter = new SeniorFilterDto
                {
                        CityId = cityId,
                        OrderStatus = orderStatus,
                        SearchText = searchText
                };

                var senior = await _service.GetSeniorsWithExtraDetailsAsync(filter);
                return Ok(senior);
        }

        [HttpGet("{seniorId}")]
        public async Task<ActionResult<SeniorDto>> GetBySeniorById(int seniorId)
        {
                var senior = await _service.GetBySeniorByIdAsync(seniorId);
                return Ok(senior);
        }


        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<SeniorDto>>> GetByCustomer(int customerId)
        {
                var seniors = await _service.GetSeniorsByCustomerAsync(customerId);
                return Ok(seniors);
        }
        [HttpPost]
        public async Task<ActionResult<SeniorDto>> Create(SeniorCreateDto dto)
        {
                var s = await _service.CreateSeniorAsync(dto);
                return CreatedAtAction(nameof(GetByCustomer), new { customerId = dto.CustomerId }, s);
        }

        /// <summary>
        /// Check if senior can be archived and get blocking item counts.
        /// </summary>
        [HttpGet("{id}/archive-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveCheckDto>> GetArchiveCheck(int id)
        {
                var check = await _service.GetArchiveCheckAsync(id);
                return Ok(check);
        }

        /// <summary>
        /// Archive a senior. If force=true, cancels all orders and sessions.
        /// </summary>
        [HttpPost("{id}/archive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> ArchiveSenior(int id, [FromBody] ArchiveRequestDto request)
        {
                var result = await _service.ArchiveSeniorAsync(id, request);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }

        /// <summary>
        /// Unarchive a senior (restore from archive).
        /// </summary>
        [HttpPost("{id}/unarchive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> UnarchiveSenior(int id)
        {
                var result = await _service.UnarchiveSeniorAsync(id);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }
}