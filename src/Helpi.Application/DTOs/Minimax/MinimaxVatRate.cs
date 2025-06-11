namespace Helpi.Application.DTOs.Minimax;

public class MinimaxVatRate
{
    public int VatRateId { get; set; }
    public string Code { get; set; } = string.Empty;
    public double Percent { get; set; }
    public MinimaxEntityReference? VatRatePercentage { get; set; }
}