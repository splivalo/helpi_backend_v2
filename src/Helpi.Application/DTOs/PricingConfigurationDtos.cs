namespace Helpi.Application.DTOs;

public class PricingConfigurationDto
{
    public int Id { get; set; }

    // Pricing
    public decimal JobHourlyRate { get; set; }
    public decimal SundayHourlyRate { get; set; }
    public decimal CompanyPercentage { get; set; }
    public decimal ServiceProviderPercentage { get; set; }

    // Cancel cutoffs (hours)
    public int StudentCancelCutoffHours { get; set; }
    public int SeniorCancelCutoffHours { get; set; }

    // Operational
    public int TravelBufferMinutes { get; set; }
    public int PaymentTimingMinutes { get; set; }

    // Tax
    public bool VatEnabled { get; set; }
    public decimal VatPercentage { get; set; }
}
