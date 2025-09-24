
namespace Helpi.Application.DTOs.Minimax;

public class MinimaxCustomer
{
    public int CustomerId { get; set; }
    public string? Code { get; set; }
    public required string Name { get; set; } = null!;
    public required string Address { get; set; } = null!;
    public required string PostalCode { get; set; } = null!;
    public required string City { get; set; } = null!;

    public required MinimaxEntityReference Country { get; set; } = null!;
    public string? CountryName { get; set; }

    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? VATIdentificationNumber { get; set; }

    public string SubjectToVAT = "N";
    public string? ConsiderCountryForBookkeeping { get; set; }

    public MinimaxEntityReference? Currency { get; set; }

    public int ExpirationDays { get; set; }
    public decimal RebatePercent { get; set; }


    public string? EInvoiceIssuing { get; set; }
    public string? InternalCustomerNumber { get; set; }
    public string? GLN { get; set; }
    public string? BudgetUserNumber { get; set; }

    public string? Usage { get; set; }
    public string? AssociationType { get; set; }

    public DateTime RecordDtModified { get; set; }
    public string? RowVersion { get; set; }
}
