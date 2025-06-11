using Helpi.Application.DTOs;


namespace Helpi.Application.Services;

public interface IDashboardService
{
    Task<List<DashboardTileData>> GetDashboardDataAsync();
}