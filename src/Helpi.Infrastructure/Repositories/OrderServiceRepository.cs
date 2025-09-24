using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;

namespace Helpi.Infrastructure.Repositories;

public class OrderServiceRepository : IOrderServiceRepository
{
    private readonly AppDbContext _context;

    public OrderServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeNoSaveAsync(IEnumerable<OrderService> orderServices)
    {
        await _context.OrderServices.AddRangeAsync(orderServices);
    }
    public void MarkForDelete(IEnumerable<OrderService> orderServices)
    {
        _context.OrderServices.RemoveRange(orderServices);
    }


}