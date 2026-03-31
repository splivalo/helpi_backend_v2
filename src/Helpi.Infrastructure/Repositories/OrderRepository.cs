namespace Helpi.Infrastructure.Repositories;

using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status = null)
        {
                var query = _context.Orders
                        .Include(o => o.Senior).ThenInclude(s => s.Contact)
                        .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                        .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Contact)
                        .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Faculty)
                        .Include(o => o.PromoCode)
                        .AsNoTracking();

                if (status.HasValue)
                        query = query.Where(o => o.Status == status.Value);

                return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
                return await _context.Orders
                .Include(o => o.Senior).ThenInclude(s => s.Contact)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Contact)
                .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Faculty)
                .Include(o => o.PromoCode)
                .FirstOrDefaultAsync(o => o.Id == id);
        }



        public async Task<IEnumerable<Order>> GetBySeniorAsync(int seniorId)
        {

                var orders = await _context.Orders
                .Where(o => o.SeniorId == seniorId)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Contact)
                .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Faculty)
                .Include(o => o.PromoCode)
                .AsNoTracking()
                .ToListAsync();

                return orders;
        }
        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
            => await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Contact)
                .Include(o => o.PromoCode)
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



        public async Task<Order?> LoadOrderWithIncludes(int orderId,
        OrderIncludeOptions options,
         bool asNoTracking = true)
        {
                var query = _context.Orders.AsQueryable();

                if (asNoTracking)
                {
                        query = query.AsNoTracking();
                }

                if (options.Senior)
                        query = query.Include(o => o.Senior).ThenInclude(s => s.Contact);

                if (options.OrderServices)
                        query = query.Include(o => o.OrderServices).ThenInclude(os => os.Service);


                if (options.Schedules)
                {
                        query = query.Include(o => o.Schedules);


                        if (options.ScheduleAssignments)
                        {

                                query = query.Include(o => o.Schedules).ThenInclude(s => s.Assignments);

                                if (options.AssignmentsJobInstances)
                                {
                                        query = query.Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.JobInstances.Where(j => !j.NeedsSubstitute));
                                }

                                if (options.ScheduleAssignmentStudent)
                                {
                                        query = query.Include(o => o.Schedules).ThenInclude(s => s.Assignments).ThenInclude(a => a.Student).ThenInclude(st => st.Contact);
                                }
                        }

                        if (options.SchedulesJobRequests)
                        {
                                query = query.Include(o => o.Schedules).ThenInclude(s => s.JobRequests);
                        }
                }


                return await query.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public Task<int> CountAsync(Expression<Func<Order, bool>> predicate)
        {
                return _context.Orders.CountAsync(predicate);
        }

        public void DetachAllEntities()
        {
                _context.DetachAllEntities();
        }
}