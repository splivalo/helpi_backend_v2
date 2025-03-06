namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class JobRequestRepository : IJobRequestRepository
{
        private readonly AppDbContext _context;

        public JobRequestRepository(AppDbContext context) => _context = context;

        public async Task<JobRequest> GetByIdAsync(int id)
            => await _context.JobRequests
                .Include(jr => jr.OrderSchedule)
                .Include(jr => jr.Student)
                .FirstOrDefaultAsync(jr => jr.Id == id);

        public async Task<IEnumerable<JobRequest>> GetPendingRequestsAsync()
            => await _context.JobRequests
                .Where(jr => jr.Status == JobRequestStatus.Pending)
                .ToListAsync();

        public async Task<IEnumerable<JobRequest>> GetExpiredRequestsAsync()
            => await _context.JobRequests
                .Where(jr => jr.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

        public async Task<JobRequest> AddAsync(JobRequest request)
        {
                await _context.JobRequests.AddAsync(request);
                await _context.SaveChangesAsync();
                return request;
        }

        public async Task UpdateAsync(JobRequest request)
        {
                _context.JobRequests.Update(request);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobRequest request)
        {
                _context.JobRequests.Remove(request);
                await _context.SaveChangesAsync();
        }
}