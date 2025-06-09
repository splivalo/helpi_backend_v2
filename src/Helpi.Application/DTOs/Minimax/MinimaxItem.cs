namespace Helpi.Application.DTOs.Minimax;

public class MinimaxItem
{
    public int ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UnitOfMeasurement { get; set; } = string.Empty;
    public double MassPerUnit { get; set; }
    public string ItemType { get; set; } = string.Empty; //  "AS", etc.

    public MinimaxEntityReference? VatRate { get; set; }
    public double Price { get; set; }
    public MinimaxEntityReference? Currency { get; set; }
    public MinimaxEntityReference? RevenueAccountDomestic { get; set; }
    public MinimaxEntityReference? RevenueAccountOutsideEU { get; set; }
    public MinimaxEntityReference? RevenueAccountEU { get; set; }
    public MinimaxEntityReference? StocksAccount { get; set; }
    public MinimaxEntityReference? ProductGroup { get; set; }
}