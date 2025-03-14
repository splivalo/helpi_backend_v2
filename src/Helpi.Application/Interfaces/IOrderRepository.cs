using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetBySeniorAsync(int seniorId);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<Order> AddNoSaveAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Order order);

    Task AddServicesToOrderAsync(int orderId, IEnumerable<OrderService> services);
}