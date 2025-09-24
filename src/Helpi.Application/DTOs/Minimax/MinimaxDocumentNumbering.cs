namespace Helpi.Application.DTOs.Minimax;

public class MinimaxDocumentNumbering
{
    public int DocumentNumberingId { get; set; }
    public string Document { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Default { get; set; } = string.Empty;
    public string Reverse { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string PackagingDepositReturnIncludedInPrice { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
    public DateTime RecordDtModified { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
