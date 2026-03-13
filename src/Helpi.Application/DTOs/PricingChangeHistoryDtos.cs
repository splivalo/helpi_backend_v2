namespace Helpi.Application.DTOs;

public class PricingChangeHistoryDto
{
    public int Id { get; set; }
    public int PricingConfigurationId { get; set; }
    public decimal OldJobHourlyRate { get; set; }
    public decimal OldSundayHourlyRate { get; set; }
    public decimal OldCompanyPercentage { get; set; }
    public decimal OldServiceProviderPercentage { get; set; }
    public decimal NewJobHourlyRate { get; set; }
    public decimal NewSundayHourlyRate { get; set; }
    public decimal NewCompanyPercentage { get; set; }
    public decimal NewServiceProviderPercentage { get; set; }
    public DateTime ChangeDate { get; set; }
    public int ChangedBy { get; set; }
    public string? Reason { get; set; }
}
