
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/contact-infos")]
public class ContactInfosController : ControllerBase
{
        private readonly ContactInfoService _service;

        public ContactInfosController(ContactInfoService service) => _service = service;

        [HttpGet("{id}")] public async Task<ActionResult<ContactInfoDto>> GetById(int id) => Ok(await _service.GetContactInfoByIdAsync(id));
        [HttpPost] public async Task<ActionResult<ContactInfoDto>> Create(ContactInfoCreateDto dto) => CreatedAtAction(nameof(GetById), new { id = (await _service.CreateContactInfoAsync(dto)).Id }, dto);
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ContactInfoUpdateDto dto)
        {
                await _service.UpdateContactInfoAsync(id, dto);
                return NoContent();
        }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { await _service.DeleteContactInfoAsync(id); return NoContent(); }

        [HttpPatch("{contactId}/language")]
        public async Task<IActionResult> UpdateLanguage(int contactId, [FromBody] UpdateLanguageDto dto)
        {
                var updated = await _service.UpdateLanguageAsync(contactId, dto.LanguageCode);
                if (!updated) return NotFound();
                return NoContent();
        }
}