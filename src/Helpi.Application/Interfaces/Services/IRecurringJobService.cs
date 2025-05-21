

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services
{
    public interface IRecurringJobService
    {
        Task GenerateFutureJobInstances();
        List<JobInstance> GenerateInstancesForAssignment(ScheduleAssignment assignment,
        int horizonMonths = 3,
        int generationThresholdDays = 14);
    }
}
