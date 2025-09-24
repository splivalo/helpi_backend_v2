using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class UserDto
{
    public int Id { get; set; }
    public UserType UserType { get; set; }
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class UserCreateDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Required]
    public UserType UserType { get; set; }
}

public class UserUpdateDto
{
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    public UserType? UserType { get; set; }
}