using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class ReviewDto
{
    public int Id { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCreateDto
{
    [Required]
    [Range(1, 5)]
    public byte Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }
}

public class ReviewUpdateDto : ReviewCreateDto { }