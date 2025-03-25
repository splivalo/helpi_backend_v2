namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class JobRequestRepository : IJobRequestRepository
{
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public JobRequestRepository(AppDbContext context, IUnitOfWork unitOfWork)
        {
                _unitOfWork = unitOfWork;
                _context = context;
        }

        public async Task<JobRequest?> GetByIdAsync(int id)
        {
                return await _context.JobRequests
                .Include(jr => jr.OrderSchedule)
                .Include(jr => jr.Student)
                .FirstOrDefaultAsync(jr => jr.Id == id);
        }

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

        public async Task<List<int>> NotifiedStudentIds(int orderScheduleId)
        {
                var notifiedStudentIds = await _context.JobRequests
                        .Where(jr => jr.OrderScheduleId == orderScheduleId)
                        .Select(jr => jr.StudentId)
                        .ToListAsync();

                return notifiedStudentIds;
        }

        public async Task<List<JobRequest>> GetStudentPendingRequests(int studentId)
        => await _context.JobRequests
                .Where(jr => jr.StudentId == studentId)
                .Where(jr => jr.Status == JobRequestStatus.Pending)
                .ToListAsync();

        public async Task<JobRequest> RespondToJobRequestAsync(JobRequest resJobRequest)
        {


                try
                {
                        var jobRequest = await _context.JobRequests
                             .Include(jr => jr.OrderSchedule)
                             .ThenInclude(os => os.Order)
                             .SingleOrDefaultAsync(jr => jr.Id == resJobRequest.Id);

                        if (jobRequest == null)
                        {
                                throw new NotFoundException("Job request not found");
                        }

                        /// TODO: figure out DomainException is good response

                        // Validate student ownership
                        if (jobRequest.StudentId != resJobRequest.StudentId)
                                throw new DomainException("Invalid student for this job request");

                        // Validate request status
                        if (jobRequest.Status != JobRequestStatus.Pending)
                                throw new DomainException("Job request is no longer active");





                        if (resJobRequest.Status == JobRequestStatus.Accepted)
                        {
                                return await HandleAcceptance(jobRequest);
                        }
                        else
                        {
                                return await HandleRejection(jobRequest, "No reason");
                        }
                }
                catch (Exception ex)
                {

                        throw;
                }
        }

        private async Task<JobRequest> HandleAcceptance(JobRequest jobRequest)
        {
                // Check if slot is still available
                var isSlotAvailable = await IsScheduleSlotAvailable(jobRequest.OrderScheduleId);

                if (!isSlotAvailable)
                        throw new DomainException("Already assigned");

                // Create schedule assignment
                var assignment = new ScheduleAssignment
                {
                        OrderScheduleId = jobRequest.OrderScheduleId,
                        StudentId = jobRequest.StudentId,
                        Status = AssignmentStatus.Accepted,
                        AssignedAt = DateTime.UtcNow
                };

                _context.ScheduleAssignments.Add(assignment);

                // Update job request
                jobRequest.Status = JobRequestStatus.Accepted;
                jobRequest.RespondedAt = DateTime.UtcNow;




                await _unitOfWork.SaveChangesAsync();



                // Decline other pending requests for same schedule
                await DeclineOtherRequests(jobRequest.OrderScheduleId, jobRequest.Id);

                return jobRequest;
        }






        private async Task<bool> IsScheduleSlotAvailable(int orderScheduleId)
        {
                return !await _context.ScheduleAssignments
                    .AnyAsync(sa => sa.OrderScheduleId == orderScheduleId
                        && sa.Status == AssignmentStatus.Accepted);
        }

        private async Task DeclineOtherRequests(int orderScheduleId, int acceptedRequestId)
        {
                var otherRequests = await _context.JobRequests
                    .Where(jr => jr.OrderScheduleId == orderScheduleId
                        && jr.Id != acceptedRequestId
                        && jr.Status == JobRequestStatus.Pending)
                    .ToListAsync();

                foreach (var request in otherRequests)
                {
                        request.Status = JobRequestStatus.AssignedToOther;
                        request.RespondedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
        }



        private async Task<JobRequest> HandleRejection(
            JobRequest jobRequest,
            string? reason
            )
        {
                jobRequest.Status = JobRequestStatus.Declined;
                jobRequest.RespondedAt = DateTime.UtcNow;
                jobRequest.RejectionReason = reason;

                await _context.SaveChangesAsync();



                // Trigger new matching attempt
                // BackgroundJob.Enqueue(() =>
                //     _matchingService.FindAndNotifyStudentsAsync(jobRequest.OrderSchedule.OrderId));

                return jobRequest;
        }




}