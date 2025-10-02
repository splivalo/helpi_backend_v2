
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudentStatisticsController : ControllerBase
{
        private readonly IStudentStatisticsService _statisticsService;

        public StudentStatisticsController(IStudentStatisticsService statisticsService)
        {
                _statisticsService = statisticsService;
        }

        [HttpGet("{studentId}/worked-hours")]
        public async Task<ActionResult<StatisticsResponse>> GetStudentStatistics(
            int studentId,
            [FromQuery] int weeksBack = 3,
            [FromQuery] int monthsBack = 3,
            [FromQuery] int yearsBack = 3)
        {
                try
                {
                        var statistics = await _statisticsService.GetStudentStatisticsAsync(
                            studentId, weeksBack, monthsBack, yearsBack);

                        return Ok(statistics);
                }
                catch (Exception ex)
                {
                        return BadRequest($"Error retrieving statistics: {ex.Message}");
                }
        }

        [HttpGet("{studentId}/worked-hours/weekly")]
        public async Task<ActionResult<List<WeekStatistics>>> GetWeeklyStatistics(
            int studentId,
            [FromQuery] int weeksBack = 3)
        {
                try
                {
                        var statistics = await _statisticsService.GetStudentStatisticsAsync(
                            studentId, weeksBack, 0, 0);

                        return Ok(statistics.Weeks);
                }
                catch (Exception ex)
                {
                        return BadRequest($"Error retrieving weekly statistics: {ex.Message}");
                }
        }

        [HttpGet("{studentId}/worked-hours/monthly")]
        public async Task<ActionResult<List<MonthStatistics>>> GetMonthlyStatistics(
            int studentId,
            [FromQuery] int monthsBack = 3)
        {
                try
                {
                        var statistics = await _statisticsService.GetStudentStatisticsAsync(
                            studentId, 0, monthsBack, 0);

                        return Ok(statistics.Months);
                }
                catch (Exception ex)
                {
                        return BadRequest($"Error retrieving monthly statistics: {ex.Message}");
                }
        }

        [HttpGet("{studentId}/worked-hours/yearly")]
        public async Task<ActionResult<List<YearStatistics>>> GetYearlyStatistics(
            int studentId,
            [FromQuery] int yearsBack = 3)
        {
                try
                {
                        var statistics = await _statisticsService.GetStudentStatisticsAsync(
                            studentId, 0, 0, yearsBack);

                        return Ok(statistics.Years);
                }
                catch (Exception ex)
                {
                        return BadRequest($"Error retrieving yearly statistics: {ex.Message}");
                }
        }
}