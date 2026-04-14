
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
        private readonly IWebHostEnvironment _env;

        public ContactInfosController(ContactInfoService service, IWebHostEnvironment env)
        {
                _service = service;
                _env = env;
        }

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

        [HttpPost("{contactId}/profile-image")]
        public async Task<IActionResult> UploadProfileImage(int contactId, IFormFile file)
        {
                try
                {
                        var url = await _service.UploadProfileImageAsync(contactId, file, _env.WebRootPath);
                        return Ok(new { profileImageUrl = url });
                }
                catch (KeyNotFoundException ex)
                {
                        return NotFound(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                        return BadRequest(new { error = ex.Message });
                }
        }

        [HttpDelete("{contactId}/profile-image")]
        public async Task<IActionResult> DeleteProfileImage(int contactId)
        {
                try
                {
                        await _service.DeleteProfileImageAsync(contactId, _env.WebRootPath);
                        return NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                        return NotFound(new { error = ex.Message });
                }
        }
}