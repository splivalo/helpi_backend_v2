using System.Text.Json.Serialization;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class DashboardTileData
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DashboardTileType Type { get; set; }
    public double Value { get; set; }
    public double? Percentage { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChangeType ChangeType { get; set; } = ChangeType.increased;
    public string Period { get; set; } = "last month";
}