

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services
{
  public interface IHangfireRecurringJobService
  {
    Task GenerateFutureJobInstances();
    List<JobInstance> GenerateInstancesForAssignment(ScheduleAssignment assignment,
      PricingConfiguration pricingConfiguration,
    int horizonMonths = 3,
    int generationThresholdDays = 14,
    DateOnly? contractEndDate = null
    );


    Task ScheduleDailyJobInstanceStatusUpdates();
  }
}
