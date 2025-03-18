
namespace Helpi.Application.Interfaces.Services;
public interface IMatchingService
{
    Task InitiateMatchingProcessAsync(int orderId);
    Task FindAndNotifyStudentsAsync(int orderId);
}