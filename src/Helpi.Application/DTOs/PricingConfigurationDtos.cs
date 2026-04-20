namespace Helpi.Application.DTOs;

public class PricingConfigurationDto
{
    public int Id { get; set; }

    // Pricing
    public decimal JobHourlyRate { get; set; }
    public decimal SundayHourlyRate { get; set; }
    public decimal StudentHourlyRate { get; set; }
    public decimal StudentSundayHourlyRate { get; set; }
    public decimal CompanyPercentage { get; set; }
    public decimal ServiceProviderPercentage { get; set; }

    // Student cancel rules
    public bool StudentCancelEnabled { get; set; }
    public int StudentCancelCutoffHours { get; set; }
    public int SeniorCancelCutoffHours { get; set; }

    // Student availability change rules
    public bool AvailabilityChangeEnabled { get; set; }
    public int AvailabilityChangeCutoffHours { get; set; }

    // Operational
    public int TravelBufferMinutes { get; set; }
    public int PaymentTimingMinutes { get; set; }
    public decimal IntermediaryPercentage { get; set; }

    // Tax
    public bool VatEnabled { get; set; }
    public decimal VatPercentage { get; set; }
}
