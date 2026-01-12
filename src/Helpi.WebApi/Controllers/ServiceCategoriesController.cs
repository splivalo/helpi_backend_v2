
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/service-categories")]
public class ServiceCategoriesController : ControllerBase
{
        private readonly ServiceCategoryService _serviceCategoryService;

        public ServiceCategoriesController(ServiceCategoryService service) => _serviceCategoryService = service;


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ServiceCategoryDto>>> GetAll()
        {
                return Ok(await _serviceCategoryService.GetAllCategoriesAsync());
        }
        [HttpPost]
        public async Task<ActionResult<ServiceCategoryDto>> Create(ServiceCategoryCreateDto dto)
        {
                var category = await _serviceCategoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetAll), category);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceCategoryDto>> Update(int id, ServiceCategoryUpdateDto dto)
        {
                var updated = await _serviceCategoryService.UpdateCategoryAsync(id, dto);

                if (updated == null)
                        return NotFound();

                return Ok(updated);
        }

        [HttpPost("upload-icon")]
        public async Task<ActionResult<ServiceCategoryIconDto>> UploadIcon([FromForm] UploadIconDto dto)
        {
                try
                {
                        var result = await _serviceCategoryService.UploadIconAsync(
                                dto.ServiceCategoryId,
                                dto.IconFiles[0]);

                        return Ok(result);
                }
                catch (ArgumentException ex)
                {
                        return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                        return StatusCode(500, new { message = "An error occurred while uploading the icon" });
                }
        }

        [HttpDelete("{id}/icon")]
        public async Task<IActionResult> DeleteIcon(int id)
        {
                try
                {
                        var result = await _serviceCategoryService.DeleteIconAsync(id);
                        if (!result)
                                return NotFound();

                        return NoContent();
                }
                catch (Exception ex)
                {
                        return StatusCode(500, new { message = "An error occurred while deleting the icon" });
                }
        }


}