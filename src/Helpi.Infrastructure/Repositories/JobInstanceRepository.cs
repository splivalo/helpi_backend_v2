namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class JobInstanceRepository : IJobInstanceRepository
{
        private readonly AppDbContext _context;

        public JobInstanceRepository(AppDbContext context) => _context = context;

        public async Task<JobInstance> GetByIdAsync(int id)
        {
                return await _context.JobInstances
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .SingleAsync(ji => ji.Id == id);
        }

        public async Task<IEnumerable<JobInstance>> GetByAssignmentAsync(int assignmentId)
            => await _context.JobInstances
                .Where(ji => ji.ScheduleAssignmentId == assignmentId)
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .ToListAsync();


        public async Task<IEnumerable<JobInstance>> GetJobInstancesByStudentAsync(int studentId)
        {
                return await _context.JobInstances
                     .Where(j => j.Assignment.StudentId == studentId)
                     .Include(j => j.Senior).ThenInclude(s => s.Contact)
                     .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                     .AsNoTracking()
                     .ToListAsync();
        }

        public async Task<IEnumerable<JobInstance>> GetJobInstances()
        {
                return await _context.JobInstances
                    .AsNoTracking()
                    .Include(j => j.Senior).ThenInclude(s => s.Contact)
                    .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                    .ToListAsync();

        }

        public async Task<IEnumerable<JobInstance>> GetUpcomingJobsAsync(DateTime cutoff)
            => await _context.JobInstances
                .Where(ji => ji.ScheduledDate >= DateOnly.FromDateTime(DateTime.UtcNow) &&
                    ji.Status == JobInstanceStatus.Upcoming &&
                    ji.StartTime <= TimeOnly.FromDateTime(cutoff))
                .AsNoTracking()
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .ToListAsync();

        public async Task<JobInstance> AddAsync(JobInstance instance)
        {
                await _context.JobInstances.AddAsync(instance);
                await _context.SaveChangesAsync();
                return instance;
        }

        public async Task UpdateAsync(JobInstance instance)
        {
                _context.JobInstances.Update(instance);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobInstance instance)
        {
                _context.JobInstances.Remove(instance);
                await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<JobInstance> jobInstances)
        {
                await _context.JobInstances.AddRangeAsync(jobInstances);
                await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobInstance>> GetSeniorCompletedJobInstances(int seniorId)
        {
                return await _context.JobInstances
                           .AsNoTracking()
                           .Where(j => j.SeniorId == seniorId && j.Status == JobInstanceStatus.Completed)
                           .Include(j => j.Senior).ThenInclude(s => s.Contact)
                           .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .ToListAsync();
        }
        public async Task<IEnumerable<JobInstance>> GetStudentCompletedJobInstances(int studentId)
        {
                return await _context.JobInstances
                           .AsNoTracking()
                           .Where(j => j.Assignment.StudentId == studentId && j.Status == JobInstanceStatus.Completed)
                           .Include(j => j.Senior).ThenInclude(s => s.Contact)
                           .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .ToListAsync();
        }
        public async Task<IEnumerable<JobInstance>> GetStudentUpComingJobInstances(int studentId)
        {
                return await _context.JobInstances
                           .AsNoTracking()
                           .Where(j => j.Assignment.StudentId == studentId && j.Status == JobInstanceStatus.Upcoming)
                           .Include(j => j.Senior).ThenInclude(s => s.Contact)
                           .Include(j => j.Assignment).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .ToListAsync();
        }

        public async Task<JobInstance?> UpdateToInProgressAsync(int jobInstanceId)
        {

                var instance = await GetByIdAsync(jobInstanceId);
                if (instance?.Status == JobInstanceStatus.Upcoming)
                {
                        instance.Status = JobInstanceStatus.InProgress;
                        await _context.SaveChangesAsync();

                        return instance;
                }

                return null;
        }

        public async Task<JobInstance?> UpdateToCompletedAsync(int jobInstanceId)
        {
                var instance = await GetByIdAsync(jobInstanceId);
                if (instance?.Status == JobInstanceStatus.InProgress)
                {
                        instance.Status = JobInstanceStatus.Completed;
                        await _context.SaveChangesAsync();

                        return instance;
                }

                return null;
        }

        public async Task<List<JobInstance>> GetByDateAsync(DateOnly today)
        {
                return await _context.JobInstances
                          .Where(j => j.ScheduledDate == today)
                          .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
                await _context.SaveChangesAsync();
        }




}

