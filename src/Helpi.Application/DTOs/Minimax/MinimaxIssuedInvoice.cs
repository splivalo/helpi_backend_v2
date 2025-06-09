namespace Helpi.Application.DTOs.Minimax;

public class MinimaxIssuedInvoice
{
    public int IssuedInvoiceId { get; set; } // "R"
    public int Year { get; set; }
    public int InvoiceNumber { get; set; }
    public MinimaxEntityReference? DocumentNumbering { get; set; }
    public required MinimaxEntityReference Customer { get; set; }
    public required DateTime DateIssued { get; set; }
    public required DateTime DateTransaction { get; set; }
    public DateTime? DateTransactionFrom { get; set; }
    public required DateTime DateDue { get; set; }
    public required string AddresseeName { get; set; }
    public required string AddresseeAddress { get; set; }
    public required string AddresseePostalCode { get; set; }
    public required string AddresseeCity { get; set; }
    public required string AddresseeCountryName { get; set; }
    public MinimaxEntityReference? AddresseeCountry { get; set; }
    public string? AddresseeGLN { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? RecipientPostalCode { get; set; }
    public string? RecipientCity { get; set; }
    public string? RecipientCountryName { get; set; }
    public required MinimaxEntityReference RecipientCountry { get; set; }
    public string? RecipientGLN { get; set; }
    public double Rabate { get; set; }
    public double ExchangeRate { get; set; }
    public string? DocumentReference { get; set; }
    public string? PaymentReference { get; set; }
    public required MinimaxEntityReference Currency { get; set; }
    public MinimaxEntityReference? Analytic { get; set; }
    public MinimaxEntityReference? Document { get; set; }
    public MinimaxEntityReference? IssuedInvoiceReportTemplate { get; set; }
    public MinimaxEntityReference? DeliveryNoteReportTemplate { get; set; }
    public string? Status { get; set; }
    public string? DescriptionAbove { get; set; }
    public string? DescriptionBelow { get; set; }
    public string? DeliveryNoteDescriptionAbove { get; set; }
    public string? DeliveryNoteDescriptionBelow { get; set; }
    public string? Notes { get; set; }
    public MinimaxEntityReference? Employee { get; set; }
    public string? PricesOnInvoice { get; set; }
    public string? RecurringInvoice { get; set; }
    public MinimaxEntityReference? InvoiceAttachment { get; set; }
    public MinimaxEntityReference? EInvoiceAttachment { get; set; }
    public required string InvoiceType { get; set; }
    public string? OriginalDocumentType { get; set; }
    public DateTime OriginalDocumentDate { get; set; }
    public string? ForwardToCRF { get; set; }
    public string? ForwardToSEF { get; set; }
    public string? ReverseReason { get; set; }
    public string? OptionalCustumerDataType { get; set; }
    public string? OptionalCustumerData { get; set; }
    public string? CustomerIDType { get; set; }
    public string? CustomerID { get; set; }
    public MinimaxEntityReference? PurposeCode { get; set; }
    public string? PaymentStatus { get; set; }
    public double InvoiceValue { get; set; }
    public double PaidValue { get; set; }
    public string? AssociationWithStock { get; set; }
    public string? DebitNote { get; set; }
    public string? DebitNoteBasis { get; set; }
    public DateTime DebitNoteBasisDate { get; set; }
    public required List<MinimaxIssuedInvoiceRow> IssuedInvoiceRows { get; set; }
    public required List<MinimaxIssuedInvoicePaymentMethod> IssuedInvoicePaymentMethods { get; set; }
    public List<MinimaxIssuedInvoiceAdditionalSourceDocument>? IssuedInvoiceAdditionalSourceDocument { get; set; }
    public DateTime RecordDtModified { get; set; }
    public string? RowVersion { get; set; }
}
