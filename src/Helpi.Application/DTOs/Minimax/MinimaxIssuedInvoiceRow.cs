using Helpi.Application.DTOs.Minimax;

public class MinimaxIssuedInvoiceRow
{
    public int IssuedInvoiceRowId { get; set; }
    public MinimaxEntityReference? IssuedInvoice { get; set; }
    public required MinimaxEntityReference Item { get; set; }
    public string? ItemName { get; set; }
    public required int RowNumber { get; set; }
    public required string ItemCode { get; set; }
    public required string SerialNumber { get; set; }
    public string? BatchNumber { get; set; }
    public string? Description { get; set; }
    public required double Quantity { get; set; }
    public required string UnitOfMeasurement { get; set; }
    public double Mass { get; set; }
    public double Price { get; set; }
    public double PriceWithVAT { get; set; }
    public double VATPercent { get; set; }
    public double Discount { get; set; }
    public double DiscountPercent { get; set; }
    public double Value { get; set; }
    public required MinimaxEntityReference VatRate { get; set; }
    public MinimaxEntityReference? VatRatePercentage { get; set; }
    public MinimaxEntityReference? Warehouse { get; set; }
    public MinimaxEntityReference? AdditionalWarehouse { get; set; }
    public double TaxFreeValue { get; set; }
    public double TaxExemptionValue { get; set; }
    public string? OtherTaxesAndDuties { get; set; }
    public string? VatAccountingType { get; set; }
    public string? TaxExemptionReasonCode { get; set; }
    public MinimaxEntityReference? Analytic { get; set; }
    public DateTime RecordDtModified { get; set; }
    public string? RowVersion { get; set; }
}
