namespace Helpi.Domain.Entities;

/// <summary>
/// Admin notes that can be attached to any entity (Senior, Student, Order).
/// </summary>
public class AdminNote
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of entity this note belongs to: "Senior", "Student", "Order"
    /// </summary>
    public string EntityType { get; set; } = null!;
    
    /// <summary>
    /// ID of the entity this note belongs to.
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// The note text content.
    /// </summary>
    public string Text { get; set; } = null!;
    
    /// <summary>
    /// When the note was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Admin user ID who created the note.
    /// </summary>
    public int CreatedByUserId { get; set; }
    
    public User? CreatedBy { get; set; }
}
