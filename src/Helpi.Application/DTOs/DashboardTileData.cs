
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class DashboardTileData
{
    public DashboardTileType Type { get; set; }
    public double Value { get; set; }
    public double? Percentage { get; set; }
    public ChangeType ChangeType { get; set; } = ChangeType.increased;
    public string Period { get; set; } = "last month";
}