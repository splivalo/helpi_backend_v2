namespace Helpi.Application.DTOs;

public class PricingConfigurationDto
{
    public int Id { get; set; }
    public decimal JobHourlyRate { get; set; }
    public decimal SundayHourlyRate { get; set; }
    public decimal CompanyPercentage { get; set; }
    public decimal ServiceProviderPercentage { get; set; }
}
