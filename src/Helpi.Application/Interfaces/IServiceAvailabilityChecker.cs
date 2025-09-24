namespace Helpi.Application.Interfaces;
public interface IServiceAvailabilityChecker
{
    Task<bool> AreServicesAvailableInRegionAsync(int regionId, IEnumerable<int> serviceIds);
}