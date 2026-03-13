using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class ReviewDto
{
    public int Id { get; set; }
    public ReviewType Type { get; set; }
    public byte Rating { get; set; }
    public int SeniorId { get; set; }
    public int JobInstanceId { get; set; }
    public string SeniorFullName { get; set; } = null!;
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public int MaxRetry { get; set; } = 2;
    public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;
    public bool IsPending { get; set; } = true;
}

public class ReviewUpdateDto
{
    public int ReviewId { get; set; }

    [Required]
    [Range(1, 5)]
    public double Rating { get; set; }
    public string? Comment { get; set; }
}

