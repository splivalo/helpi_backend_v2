namespace Helpi.Infrastructure.Repositories;

using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.EntityFrameworkCore;

public class JobInstanceRepository : IJobInstanceRepository
{
        private readonly AppDbContext _context;

        public JobInstanceRepository(AppDbContext context) => _context = context;

        public async Task<JobInstance?> GetByIdSlimAsync(int id)
        {
                return await _context.JobInstances.AsNoTracking()
                .SingleAsync(ji => ji.Id == id);
        }
        public async Task<JobInstance> GetByIdAsync(int id)
        {
                return await _context.JobInstances
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(j => j.Reviews)
                .SingleAsync(ji => ji.Id == id);
        }

        public async Task<IEnumerable<JobInstance>> GetByAssignmentAsync(int assignmentId)
            => await _context.JobInstances
             .Where(j => j.NeedsSubstitute == false)
                .Where(ji => ji.ScheduleAssignmentId == assignmentId)
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(j => j.Reviews)
                .OrderByDescending(j => j.ScheduledDate)
                .ToListAsync();

        public async Task<IEnumerable<JobInstance>> GetByOrderIdAsync(int orderId)
            => await GetByOrderIdAsync(orderId, null, null);

        public async Task<IEnumerable<JobInstance>> GetByOrderIdAsync(int orderId, DateOnly? from, DateOnly? to)
        {
                var query = _context.JobInstances
                    .Where(j => j.NeedsSubstitute == false)
                    .Where(j => j.OrderId == orderId);

                if (from.HasValue)
                        query = query.Where(j => j.ScheduledDate >= from.Value);
                if (to.HasValue)
                        query = query.Where(j => j.ScheduledDate <= to.Value);

                return await query
                    .Include(j => j.Senior).ThenInclude(s => s.Contact)
                    .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                    .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                    .Include(j => j.Reviews)
                    .OrderBy(j => j.ScheduledDate).ThenBy(j => j.StartTime)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<IEnumerable<JobInstance>> GetJobInstancesByStudentAsync(int studentId)
        {
                return await _context.JobInstances
                     .Where(j => j.NeedsSubstitute == false)
                     .Where(j => j.ScheduleAssignment != null && j.ScheduleAssignment.StudentId == studentId)
                     .Where(j => j.ScheduleAssignment!.Status == AssignmentStatus.Accepted
                               || j.ScheduleAssignment!.Status == AssignmentStatus.PendingAcceptance
                               || j.ScheduleAssignment!.Status == AssignmentStatus.Completed
                               // Include Terminated assignments so cancelled sessions remain visible
                               // in the student app with "Otkazano" status instead of disappearing.
                               || (j.ScheduleAssignment!.Status == AssignmentStatus.Terminated
                                   && j.Status == JobInstanceStatus.Cancelled))
                     .Include(j => j.Senior).ThenInclude(s => s.Contact)
                     .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                     .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                     .Include(j => j.Reviews)
                     .AsNoTracking()
                     .OrderByDescending(j => j.ScheduledDate)
                     .ToListAsync();
        }

        public async Task<IEnumerable<JobInstance>> GetJobInstances()
        {
                return await _context.JobInstances
                    .AsNoTracking()
                    .Include(j => j.Senior).ThenInclude(s => s.Contact)
                                        .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                    .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                    .Include(j => j.Reviews)
                    .OrderByDescending(j => j.ScheduledDate)
                    .ToListAsync();

        }

        public async Task<IEnumerable<JobInstance>> GetUpcomingJobsAsync(DateTime cutoff)
            => await _context.JobInstances
                .Where(ji => ji.ScheduledDate >= DateOnly.FromDateTime(DateTime.UtcNow) &&
                    ji.Status == JobInstanceStatus.Upcoming &&
                    ji.StartTime <= TimeOnly.FromDateTime(cutoff))
                .AsNoTracking()
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                                .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(j => j.Reviews)
                .OrderByDescending(j => j.ScheduledDate)
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

        public async Task UpdateRangeAsync(List<JobInstance> jobInstances)
        {
                _context.JobInstances.UpdateRange(jobInstances);
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
                           .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .Include(j => j.Reviews)
                           .OrderByDescending(j => j.ScheduledDate)
                           .ToListAsync();
        }
        public async Task<IEnumerable<JobInstance>> GetStudentCompletedJobInstances(int studentId)
        {
                return await _context.JobInstances
                           .AsNoTracking()
                            .Where(j => j.NeedsSubstitute == false)
                           .Where(j => j.ScheduleAssignment != null && j.ScheduleAssignment.StudentId == studentId && j.Status == JobInstanceStatus.Completed)
                           .Include(j => j.Senior).ThenInclude(s => s.Contact)
                           .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .Include(j => j.Reviews)
                           .OrderByDescending(j => j.ScheduledDate)
                           .ToListAsync();
        }
        public async Task<IEnumerable<JobInstance>> GetStudentUpComingJobInstances(int studentId)
        {
                return await _context.JobInstances
                           .AsNoTracking()
                           .Where(j => j.NeedsSubstitute == false)
                           .Where(j => j.ScheduleAssignment != null && j.ScheduleAssignment.StudentId == studentId && j.Status == JobInstanceStatus.Upcoming)
                           .Where(j => j.ScheduleAssignment!.Status == AssignmentStatus.Accepted
                                     || j.ScheduleAssignment!.Status == AssignmentStatus.PendingAcceptance)
                           .Include(j => j.Senior).ThenInclude(s => s.Contact)
                           .Include(j => j.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact)
                           .OrderByDescending(j => j.ScheduledDate)
                           .ToListAsync();
        }

        public async Task<JobInstance?> UpdateToInProgressAsync(int jobInstanceId)
        {

                var instance = await GetByIdAsync(jobInstanceId);
                if (instance?.Status != JobInstanceStatus.Upcoming) return null;
                // TODO(stripe-integration): Uncomment PaymentStatus check when Stripe is fully integrated.
                // Without Stripe credentials, ProcessPaymentAsync cannot charge → PaymentStatus stays Pending
                // → this blocks the entire InProgress→Completed flow. Re-enable after Stripe go-live.
                // See also: helpi_app/lib/core/constants/pricing.dart for matching TODO.
                // if (instance?.PaymentStatus != PaymentStatus.Paid) return null;

                instance.Status = JobInstanceStatus.InProgress;
                await _context.SaveChangesAsync();

                return instance;

        }

        // public async Task<JobInstance?> UpdateToCompletedAsync(int jobInstanceId)
        // {
        //         var instance = await GetByIdAsync(jobInstanceId);
        //         if (instance?.Status == JobInstanceStatus.InProgress)
        //         {
        //                 instance.Status = JobInstanceStatus.Completed;
        //                 await _context.SaveChangesAsync();

        //                 return instance;
        //         }

        //         return null;
        // }

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

        public async Task<JobInstance?> LoadJobInstanceWithIncludes(int jobInstanceId, SessionIncludeOptions includes)
        {
                var query = _context.JobInstances.AsQueryable();

                if (includes.Senior)
                        query = query.Include(ji => ji.Senior).ThenInclude(s => s.Contact);


                if (includes.Order)
                {

                        query = query.Include(ji => ji.Order);

                        if (includes.OrderPaymentMethod)
                        {
                                query = query.Include(ji => ji.Order).ThenInclude(o => o.PaymentMethod);
                        }

                        if (includes.OrderServices)
                        {
                                query = query.Include(ji => ji.Order).ThenInclude(o => o.OrderServices);
                        }



                }

                if (includes.OrderSchedule)
                {
                        query = query.Include(ji => ji.OrderSchedule);

                }

                if (includes.Assignment)
                {
                        query = query
                                                        .Include(ji => ji.ScheduleAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact);

                        if (includes.AssignmentOrderSchedule)
                        {
                                query = query
                                                                          .Include(ji => ji.ScheduleAssignment!).ThenInclude(a => a.OrderSchedule);
                        }

                }

                if (includes.PrevAssignment)
                {
                        query = query
                                                        .Include(ji => ji.PrevAssignment!).ThenInclude(a => a.Student).ThenInclude(s => s.Contact);

                        if (includes.PrevAssignmentOrderSchedule)
                        {
                                query = query
                                                                          .Include(ji => ji.PrevAssignment!).ThenInclude(a => a.OrderSchedule);
                        }

                }


                return await query.FirstOrDefaultAsync(ji => ji.Id == jobInstanceId);
        }

        public Task<int> SumAsync(Expression<Func<JobInstance, bool>> predicate, Expression<Func<JobInstance, int>> selector)
        {
                return _context.JobInstances.Where(predicate).SumAsync(selector);
        }

        public async Task<List<JobInstance>> GetJobInstancesAsync(
                int? assignmentId,
                int? prevAssignmentId,
                JobInstanceStatus? status,
                SessionIncludeOptions options
     )
        {
                var query = _context.JobInstances

                    .AsNoTracking()
                    .AsQueryable();

                if (assignmentId.HasValue)
                {
                        query = query.Where(j => j.ScheduleAssignmentId == assignmentId.Value);
                }

                if (prevAssignmentId.HasValue)
                {
                        query = query.Where(j => j.PrevAssignmentId == prevAssignmentId.Value);
                }

                if (status.HasValue)
                {
                        query = query.Where(j => j.Status == status.Value);
                }

                if (options.Senior)
                {
                        query = query.Include(j => j.Senior)
                                     .ThenInclude(s => s.Contact);
                }

                if (options.Assignment)
                {
                        query = query.Include(j => j.ScheduleAssignment);

                        if (options.AssignmentStudent)
                        {
                                query = query.Include(j => j.ScheduleAssignment)
                                             .ThenInclude(a => a!.Student)
                                             .ThenInclude(s => s!.Contact);
                        }
                }

                if (options.Order)
                {
                        query = query.Include(j => j.Order);

                        if (options.OrderPaymentMethod)
                        {
                                query = query.Include(j => j.Order)
                                             .ThenInclude(o => o.PaymentMethod);
                        }
                }

                return await query.OrderByDescending(j => j.ScheduledDate)
                                .ToListAsync();
        }

        public void Detach(JobInstance jobInstance)
        {
                _context.DetachEntity(jobInstance);
        }

        public async Task<List<JobInstance>> GetFromDateForScheduleAsync(DateOnly fromDate, int scheduleId)
        {
                return await _context.JobInstances
                .Where(j => j.OrderScheduleId == scheduleId && j.ScheduledDate >= fromDate)
                .ToListAsync();
        }

        public void MarkForDeleteRange(IEnumerable<JobInstance> jobs)
        {
                _context.JobInstances.RemoveRange(jobs);
        }

        public async Task<IEnumerable<JobInstance>> GetCompletedJobInstancesForStudentAsync(int studentId, DateTime fromDate, DateTime toDate)
        {
                return await _context.JobInstances
                    .Include(ji => ji.ScheduleAssignment)
                                        .Where(ji => ji.ScheduleAssignment!.StudentId == studentId)
                    .Where(ji => ji.Status == JobInstanceStatus.Completed)
                    .Where(ji => !ji.NeedsSubstitute)
                    .Where(ji => ji.ScheduledDate >= DateOnly.FromDateTime(fromDate) &&
                                ji.ScheduledDate <= DateOnly.FromDateTime(toDate))
                    .ToListAsync();
        }

        public async Task<decimal> GetTotalCompletedHoursForPeriodAsync(int studentId, DateTime startDate, DateTime endDate)
        {
                var jobs = await _context.JobInstances
                    .Include(ji => ji.ScheduleAssignment)
                                        .Where(ji => ji.ScheduleAssignment!.StudentId == studentId)
                    .Where(ji => ji.Status == JobInstanceStatus.Completed)
                    .Where(ji => !ji.NeedsSubstitute)
                    .Where(ji => ji.ScheduledDate >= DateOnly.FromDateTime(startDate) &&
                                ji.ScheduledDate <= DateOnly.FromDateTime(endDate))
                    .ToListAsync();

                return jobs.Sum(ji => ji.DurationHours);
        }

}

