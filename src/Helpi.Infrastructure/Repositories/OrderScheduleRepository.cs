namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class OrderScheduleRepository : IOrderScheduleRepository
{
        private readonly AppDbContext _context;

        public OrderScheduleRepository(AppDbContext context) => _context = context;

        public async Task<OrderSchedule> GetByIdAsync(int id)
            => await _context.OrderSchedules
                .Include(os => os.Order)
                .FirstOrDefaultAsync(os => os.Id == id);

        public async Task<IEnumerable<OrderSchedule>> GetByOrderAsync(int orderId)
            => await _context.OrderSchedules
                .Where(os => os.OrderId == orderId)
                .ToListAsync();

        public async Task<IEnumerable<OrderSchedule>> GetActiveSchedulesAsync(DateOnly date)
            => await _context.OrderSchedules
                .Where(os => os.Order.StartDate <= date &&
                    (os.Order.EndDate == null || os.Order.EndDate >= date))
                .ToListAsync();

        public async Task<OrderSchedule> AddAsync(OrderSchedule schedule)
        {
                await _context.OrderSchedules.AddAsync(schedule);
                await _context.SaveChangesAsync();
                return schedule;
        }

        public async Task UpdateAsync(OrderSchedule schedule)
        {
                _context.OrderSchedules.Update(schedule);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(OrderSchedule schedule)
        {
                _context.OrderSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
        }
}