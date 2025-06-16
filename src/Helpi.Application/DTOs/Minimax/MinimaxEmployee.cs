namespace Helpi.Application.DTOs.Minimax;

public class MinimaxEmployee
{
    public int EmployeeId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? TaxNumber { get; set; }
    public required string EmploymentType { get; set; }

    public DateTime? EmploymentStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }

    public MinimaxEntityReference? Country { get; set; }
    public MinimaxEntityReference? CountryOfResidence { get; set; }
}
