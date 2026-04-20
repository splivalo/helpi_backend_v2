namespace Helpi.Domain.Entities;


/// <summary>
/// should only have 1 record (the active pricing config)
/// </summary>
public class PricingConfiguration
{
    public int Id { get; set; }

    // ── Pricing ──
    public decimal JobHourlyRate { get; set; }
    public decimal SundayHourlyRate { get; set; }
    public decimal StudentHourlyRate { get; set; } = 7.40m;
    public decimal StudentSundayHourlyRate { get; set; } = 11.10m;
    public decimal CompanyPercentage { get; set; }
    public decimal ServiceProviderPercentage { get; set; }

    // ── Student cancel rules ──
    public bool StudentCancelEnabled { get; set; } = true;
    public int StudentCancelCutoffHours { get; set; } = 6;
    public int SeniorCancelCutoffHours { get; set; } = 1;

    // ── Student availability change rules ──
    public bool AvailabilityChangeEnabled { get; set; } = true;
    public int AvailabilityChangeCutoffHours { get; set; } = 24;

    // ── Operational ──
    public int TravelBufferMinutes { get; set; } = 15;
    public int PaymentTimingMinutes { get; set; } = 30;
    public decimal IntermediaryPercentage { get; set; } = 18;

    // ── Tax ──
    public bool VatEnabled { get; set; } = false;
    public decimal VatPercentage { get; set; } = 0;

    public bool IsValidSplit => CompanyPercentage + ServiceProviderPercentage == 100;
}
