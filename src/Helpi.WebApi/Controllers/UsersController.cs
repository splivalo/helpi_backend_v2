
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
        private readonly UserService _service;

        public UsersController(UserService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<UserDto>>> GetAll() => Ok(await _service.GetAllUsersAsync());
        [HttpGet("{id}")] public async Task<ActionResult<UserDto>> GetById(int id) => Ok(await _service.GetUserByIdAsync(id));
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create(UserCreateDto dto)
        {
                var created = await _service.CreateUserAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        [HttpPut("{id}")] public async Task<IActionResult> Update(int id, UserUpdateDto dto) { await _service.UpdateUserAsync(id, dto); return NoContent(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { await _service.DeleteUserAsync(id); return NoContent(); }
}