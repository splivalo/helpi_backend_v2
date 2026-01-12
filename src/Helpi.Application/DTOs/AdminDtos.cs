using System.ComponentModel.DataAnnotations;
using Helpi.Application.DTOs;

public class AdminDto
{
    public int UserId { get; set; }
    [Required]
    public int ContactId { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;
}