namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class OrderScheduleRepository : IOrderScheduleRepository
{
        private readonly AppDbContext _context;

        public OrderScheduleRepository(AppDbContext context) => _context = context;

        public async Task<OrderSchedule?> GetByIdAsync(int id)
        {
                return await _context.OrderSchedules
             .Include(os => os.Order)
             .SingleOrDefaultAsync(os => os.Id == id);
        }
        public async Task<List<OrderSchedule>> LoadWithIncudes(int? scheduleId, OrderScheduleInculdes inculdes)
        {

                var query = _context.OrderSchedules.AsNoTracking();

                if (scheduleId != null)
                {
                        query = query.Where(os => os.Id == scheduleId);
                }

                if (inculdes.JobRequests)
                {
                        query = query.Include(os => os.JobRequests);
                }

                if (inculdes.Assignments)
                {
                        query = query.Include(os => os.Assignments);

                        if (inculdes.AssignmentsJobInstances)
                        {
                                query = query.Include(os => os.Assignments).ThenInclude(a => a.JobInstances);
                        }
                }



                return await query.ToListAsync();
        }

        public async Task<IEnumerable<OrderSchedule>> GetByOrderAsync(int orderId)
        {
                return await _context.OrderSchedules
                .Where(os => os.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderSchedule>> GetActiveSchedulesAsync(DateOnly date)
        {
                return await _context.OrderSchedules
                .Where(os => os.Order.StartDate <= date &&
                    (os.Order.EndDate >= date))
                .ToListAsync();
        }

        public async Task<OrderSchedule> AddNoSaveAsync(OrderSchedule schedule)
        {
                await _context.OrderSchedules.AddAsync(schedule);
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

        public async Task AddRangeNoSaveAsync(IEnumerable<OrderSchedule> orderSchedules)
        {
                await _context.OrderSchedules.AddRangeAsync(orderSchedules);
        }

        public async Task<IEnumerable<OrderSchedule>> GetFailedAutoSchedulingSchedules()
        {
                var assignedStatuses = new[]
                {
                        AssignmentStatus.Accepted,
                        AssignmentStatus.Completed
                };

                return await _context.OrderSchedules
                    .Where(os => !os.AllowAutoScheduling
                                && !os.IsCancelled
                                && !os.Assignments.Any(a =>
                                    assignedStatuses.Contains(a.Status) &&
                                    !a.IsJobInstanceSub))
                    .ToListAsync();
        }

        public void MarkForDelete(OrderSchedule schedule)
        {
                throw new NotImplementedException();
        }

        public void MarkForsDelete(OrderSchedule schedule)
        {
                _context.OrderSchedules.Remove(schedule);
        }

}
