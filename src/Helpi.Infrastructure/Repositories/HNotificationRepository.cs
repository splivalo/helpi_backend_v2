using Microsoft.EntityFrameworkCore;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Helpi.Application.Interfaces;

namespace Helpi.Infrastructure.Repositories;

public class HNotificationRepository : IHNotificationRepository
{
    private readonly AppDbContext _context;

    public HNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HNotification?> GetByIdAsync(int id)
    {
        return await _context.HNotifications
                    .Include(n => n.Senior).ThenInclude(s => s.Contact)
            .Include(n => n.Student).ThenInclude(st => st.Contact)
                    .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<HNotification>> GetByUserIdAsync(int userId)
    {
        return await _context.HNotifications
            .Where(n => n.RecieverUserId == userId)
            .Include(n => n.Senior).ThenInclude(s => s.Contact)
            .Include(n => n.Student).ThenInclude(st => st.Contact)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<HNotification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _context.HNotifications
            .Where(n => n.RecieverUserId == userId && !n.IsRead)
                     .Include(n => n.Senior).ThenInclude(s => s.Contact)
            .Include(n => n.Student).ThenInclude(st => st.Contact)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<HNotification> CreateAsync(HNotification notification)
    {
        _context.HNotifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<HNotification> UpdateAsync(HNotification notification)
    {
        _context.HNotifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task DeleteAsync(int id)
    {
        var notification = await _context.HNotifications.FindAsync(id);
        if (notification != null)
        {
            _context.HNotifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var notification = await _context.HNotifications.FindAsync(id);
        if (notification == null)
            return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        await _context.HNotifications
            .Where(n => n.RecieverUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

        return true;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.HNotifications
            .CountAsync(n => n.RecieverUserId == userId && !n.IsRead);
    }

    public async Task<IEnumerable<HNotification>> GetPagedAsync(int userId, int page, int pageSize)
    {
        return await _context.HNotifications
            .Where(n => n.RecieverUserId == userId)
                    .Include(n => n.Senior)
                    .Include(n => n.Student)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}

