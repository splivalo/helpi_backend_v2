using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Helpi.Application.DTOs;

public class StudentContractDto
{
    public int Id { get; set; }
    public string CloudPath { get; set; } = null!;

    public int StudentId { get; set; }
    public ContractStatus Status { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }

    public string ContractNumber { get; set; } = null!;
}

public class StudentContractCreateDto
{
    [Required]
    public IFormFile[] ContractFile { get; set; } = null!;

    [Required]
    public int StudentId { get; set; }

    [Required]
    public DateOnly EffectiveDate { get; set; }

    [Required]
    public DateOnly ExpirationDate { get; set; }
}

public class StudentContractUpdateDto
{


    public int StudentId { get; set; }

    public IFormFile? NewContractFile { get; set; }

    public DateOnly? EffectiveDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
}