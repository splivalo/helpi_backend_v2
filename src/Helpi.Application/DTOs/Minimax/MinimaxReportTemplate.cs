
namespace Helpi.Application.DTOs.Minimax;

public class MinimaxReportTemplate
{
    public int ReportTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayType { get; set; } = string.Empty;
    public string Default { get; set; } = string.Empty;
    public DateTime RecordDtModified { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}