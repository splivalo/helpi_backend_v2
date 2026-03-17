using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Helpi.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin-notes")]
public class AdminNotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminNotesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all notes for a specific entity.
    /// </summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<List<AdminNoteDto>>> GetNotes(string entityType, int entityId)
    {
        var notes = await _context.AdminNotes
            .Where(n => n.EntityType == entityType && n.EntityId == entityId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new AdminNoteDto
            {
                Id = n.Id,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                Text = n.Text,
                CreatedAt = n.CreatedAt,
                CreatedByUserId = n.CreatedByUserId,
                CreatedByName = n.CreatedBy != null ? n.CreatedBy.Email : null
            })
            .ToListAsync();

        return Ok(notes);
    }

    /// <summary>
    /// Create a new admin note.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AdminNoteDto>> CreateNote([FromBody] AdminNoteCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var id) ? id : 0;

        var note = new AdminNote
        {
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.AdminNotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotes), new { entityType = note.EntityType, entityId = note.EntityId }, new AdminNoteDto
        {
            Id = note.Id,
            EntityType = note.EntityType,
            EntityId = note.EntityId,
            Text = note.Text,
            CreatedAt = note.CreatedAt,
            CreatedByUserId = note.CreatedByUserId
        });
    }

    /// <summary>
    /// Update an existing note.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AdminNoteDto>> UpdateNote(int id, [FromBody] AdminNoteUpdateDto dto)
    {
        var note = await _context.AdminNotes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        note.Text = dto.Text;
        await _context.SaveChangesAsync();

        return Ok(new AdminNoteDto
        {
            Id = note.Id,
            EntityType = note.EntityType,
            EntityId = note.EntityId,
            Text = note.Text,
            CreatedAt = note.CreatedAt,
            CreatedByUserId = note.CreatedByUserId
        });
    }

    /// <summary>
    /// Delete a note.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var note = await _context.AdminNotes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        _context.AdminNotes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
