
namespace Helpi.Application.DTOs.Minimax;

public class MinimaxIssuedInvoiceAdditionalSourceDocument
{
    public int IssuedInvoiceAdditionalSourceDocumentId { get; set; }
    public MinimaxEntityReference? IssuedInvoice { get; set; }
    public string? SourceDocumentType { get; set; }
    public DateTime SourceDocumentDate { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public DateTime RecordDtModified { get; set; }
    public string? RowVersion { get; set; }
}