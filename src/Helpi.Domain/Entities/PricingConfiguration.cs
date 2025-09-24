namespace Helpi.Domain.Entities;


/// <summary>
/// should only have 1 record (the active pricing config)
/// </summary>
public class PricingConfiguration
{
    public int Id { get; set; }
    public decimal JobHourlyRate { get; set; }
    public decimal CompanyPercentage { get; set; }
    public decimal ServiceProviderPercentage { get; set; }

    public bool IsValidSplit => CompanyPercentage + ServiceProviderPercentage == 100;
}
