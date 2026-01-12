using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IStudentStatisticsService
{
    Task<StatisticsResponse> GetStudentStatisticsAsync(int studentId, int weeksBack, int monthsBack, int yearsBack);
}