using Hangfire;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

public class JobRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public JobRunner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task InitiateMatchingProcessAsync(int orderId)
    {
        using var scope = _scopeFactory.CreateScope();
        var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
        await matchingService.InitiateMatchingProcessAsync(orderId);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ProcessJobInstanceMatchingAsync(
                         int jobInstanceId,
                         int reassignmentRecordId
                            )
    {
        using var scope = _scopeFactory.CreateScope();
        var jobMatchingService = scope.ServiceProvider.GetRequiredService<IJobInstanceMatchingService>();
        await jobMatchingService.ProcessJobInstanceMatchingAsync(jobInstanceId, reassignmentRecordId);
    }
}
