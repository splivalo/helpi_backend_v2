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

    public DateTime? DeletedOn { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }

    public string ContractNumber { get; set; } = null!;

    public ICollection<SessionDto> Sessions { get; set; } = new List<SessionDto>();
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

    public ContractStatus Status
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < EffectiveDate)
                return ContractStatus.Pending;

            if (today > ExpirationDate)
                return ContractStatus.Expired;

            return ContractStatus.Active;
        }
    }

}

public class StudentContractUpdateDto
{

    public DateTime? DeletedOn { get; set; }


    public IFormFile[]? NewContractFile { get; set; }

    public DateOnly? EffectiveDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
}



public class CompletedStudentContractDto
{
    public int Id { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }

    // Pre-calculated summary
    public int TotalJobs { get; set; }
    public decimal DurationHours { get; set; }
    public decimal TotalStudentEarnings { get; set; }
    public decimal TotalCompanyEarnings { get; set; }

    // Completed JobInstances
    public ICollection<CompletedSessionDto> CompletedJobs { get; set; } = new List<CompletedSessionDto>();
}
