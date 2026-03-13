using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<List<DashboardTileData>>> GetAdminDashboard()
    {
        return Ok(await _dashboardService.GetDashboardDataAsync());
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<List<DashboardTileData>>> GetStudentDashboard(int studentId)
    {
        return Ok(await _dashboardService.GetStudentDashboardAsync(studentId));
    }

    [HttpGet("senior/{seniorId}")]
    public async Task<ActionResult<List<DashboardTileData>>> GetSeniorDashboard(int seniorId)
    {
        return Ok(await _dashboardService.GetSeniorDashboardAsync(seniorId));
    }
}