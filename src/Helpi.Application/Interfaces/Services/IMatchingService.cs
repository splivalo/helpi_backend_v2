
namespace Helpi.Application.Interfaces.Services;
public interface IMatchingService
{
    void StartMatching(int orderId);
    Task InitiateMatchingProcessAsync(int orderId);
    Task FindAndNotifyStudentsAsync(int orderId);
}