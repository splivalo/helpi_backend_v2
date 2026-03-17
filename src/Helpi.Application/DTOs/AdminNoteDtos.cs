using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

public class AdminNoteDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = null!;
    public int EntityId { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
}

public class AdminNoteCreateDto
{
    [Required]
    public string EntityType { get; set; } = null!;
    
    [Required]
    public int EntityId { get; set; }
    
    [Required]
    [MinLength(1)]
    public string Text { get; set; } = null!;
}

public class AdminNoteUpdateDto
{
    [Required]
    [MinLength(1)]
    public string Text { get; set; } = null!;
}
