
namespace Helpi.Application.DTOs.Minimax;

public class MinimaxContact
{
    public int ContactId { get; set; }

    public required MinimaxEntityReference Customer { get; set; }

    public required string FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Fax { get; set; }

    public string? MobilePhone { get; set; }

    public required string Email { get; set; }

    public string? Notes { get; set; }

    public string? Default { get; set; } = "D";

    public DateTime? RecordDtModified { get; set; }

    public string? RowVersion { get; set; }
}


