using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IOrderScheduleRepository
{
    Task<OrderSchedule?> GetByIdAsync(int id);
    Task<IEnumerable<OrderSchedule>> GetByOrderAsync(int orderId);
    Task<IEnumerable<OrderSchedule>> GetActiveSchedulesAsync(DateOnly date);
    Task<OrderSchedule> AddNoSaveAsync(OrderSchedule schedule);
    Task UpdateAsync(OrderSchedule schedule);
    Task DeleteAsync(OrderSchedule schedule);
    Task AddRangeNoSaveAsync(IEnumerable<OrderSchedule> orderSchedules);
    Task<IEnumerable<OrderSchedule>> GetFailedAutoSchedulingSchedules();
}