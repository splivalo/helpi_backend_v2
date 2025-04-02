namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) => _context = context;

        public async Task<Order?> GetByIdAsync(int id)
        {
                return await _context.Orders
                .Include(o => o.Senior)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetBySeniorAsync(int seniorId)
        {

                var orders = await _context.Orders
                .Where(o => o.SeniorId == seniorId)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules)
                .AsNoTracking()
                .ToListAsync();

                return orders;
        }
        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
            => await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules)
                .ToListAsync();

        public async Task<Order> AddNoSaveAsync(Order order)
        {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                return order;
        }

        public async Task UpdateAsync(Order order)
        {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order)
        {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
        }

        public Task AddServicesToOrderAsync(int orderId, IEnumerable<OrderService> services)
        {
                throw new NotImplementedException();
        }



}