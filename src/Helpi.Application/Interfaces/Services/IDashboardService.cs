using Helpi.Application.DTOs;


namespace Helpi.Application.Services;

public interface IDashboardService
{
    Task<List<DashboardTileData>> GetAdminDashboardAsync();
    Task<List<DashboardTileData>> GetStudentDashboardAsync(int studentId);
    Task<List<DashboardTileData>> GetSeniorDashboardAsync(int seniorId);
}