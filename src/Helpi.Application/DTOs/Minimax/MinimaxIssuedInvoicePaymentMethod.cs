namespace Helpi.Application.DTOs.Minimax;

public class MinimaxIssuedInvoicePaymentMethod
{
    public int IssuedInvoicePaymentMethodId { get; set; }
    public MinimaxEntityReference? IssuedInvoice { get; set; }
    public required MinimaxEntityReference PaymentMethod { get; set; }
    public MinimaxEntityReference? CashRegister { get; set; }
    public MinimaxEntityReference? Revenue { get; set; }
    public DateTime? RevenueDate { get; set; }
    public required double Amount { get; set; }
    public double AmountInDomesticCurrency { get; set; }
    public required string AlreadyPaid { get; set; }
    public DateTime RecordDtModified { get; set; }
    public string? RowVersion { get; set; }
}