using Helpi.Application.DTOs;


namespace Helpi.Application.Services;

public interface IDashboardService
{
    Task<List<DashboardTileData>> GetDashboardDataAsync();
    Task<List<DashboardTileData>> GetStudentDashboardAsync(int studentId);
    Task<List<DashboardTileData>> GetSeniorDashboardAsync(int seniorId);
}