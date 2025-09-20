

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IOrderServiceRepository
{
    Task AddRangeNoSaveAsync(IEnumerable<OrderService> orderServices);
    void MarkForDelete(IEnumerable<OrderService> orderServices);
}
