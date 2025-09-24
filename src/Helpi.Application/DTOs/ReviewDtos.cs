using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class ReviewDto
{
    public int Id { get; set; }
    public byte Rating { get; set; }
    public int SeniorId { get; set; }
    public int JobInstanceId { get; set; }
    public string SeniorFullName { get; set; } = null!;
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCreateDto
{
    [Required]
    [Range(1, 5)]
    public byte Rating { get; set; }


    public int JobInstanceId { get; set; }
    public int SeniorId { get; set; }
    [StringLength(200)]
    public string SeniorFullName { get; set; } = null!;

    public int StudentId { get; set; }
    [StringLength(200)]
    public string StudentFullName { get; set; } = null!;

    [StringLength(2000)]
    public string? Comment { get; set; }
}

public class ReviewUpdateDto : ReviewCreateDto { }