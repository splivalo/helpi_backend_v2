
using AutoMapper;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class JobRequestService
{
        private readonly IJobRequestRepository _jobRequestRepository;
        private readonly OrderStatusMaintenanceService _statusMaintenanceService;
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobRequestService> _logger;

        private readonly IHangfireRecurringJobService _recurringJobService;
        private readonly IPricingConfigurationRepository _pricingConfigRepo;

        private readonly IReassignmentService _reassignmentService;

        public JobRequestService(
                IJobRequestRepository jobRequestRepository,
                OrderStatusMaintenanceService statusMaintenanceService,
                IJobInstanceRepository jobInstanceRepository,
                IMapper mapper,
                 IHangfireRecurringJobService recurringJobService,
                 IPricingConfigurationRepository pricingConfigRepo,
                  ILogger<JobRequestService> logger,
                     IReassignmentService reassignmentService
                     )
        {
                _jobRequestRepository = jobRequestRepository;
                _statusMaintenanceService = statusMaintenanceService;
                _jobInstanceRepository = jobInstanceRepository;
                _mapper = mapper;
                _recurringJobService = recurringJobService;
                _pricingConfigRepo = pricingConfigRepo;
                _logger = logger;
                _reassignmentService = reassignmentService;
        }

        public async Task<List<JobRequestDto>> GetStudentPendingRequests(int studentId)
        {

                var jobRequestsDtos = await _jobRequestRepository.GetStudentPendingRequests(studentId);
                return _mapper.Map<List<JobRequestDto>>(jobRequestsDtos);

        }

        public async Task<List<JobRequestDto>> GetStudentRequests(int studentId)
        {
                var jobRequestsDtos = await _jobRequestRepository.GetStudentRequests(studentId);
                return _mapper.Map<List<JobRequestDto>>(jobRequestsDtos);
        }

        public async Task<JobRequestDto> CreateJobRequestAsync(JobRequestCreateDto dto)
        {
                var request = _mapper.Map<JobRequest>(dto);
                await _jobRequestRepository.AddAsync(request);
                return _mapper.Map<JobRequestDto>(request);
        }

        public async Task<JobRequestDto> RespondToJobRequestAsync(RespondToJobRequestDto respondToJobRequestDto)
        {
                var jobRequest = _mapper.Map<JobRequest>(respondToJobRequestDto);

                jobRequest = await _jobRequestRepository.RespondToJobRequestAsync(jobRequest);

                if (jobRequest.Status == JobRequestStatus.Accepted)
                {

                        // Handle reassignment completion if this is a reassignment
                        if (jobRequest.IsReassignment && jobRequest.ReassignmentRecordId.HasValue)
                        {
                                await _reassignmentService.CompleteReassignment(
                                    jobRequest.ReassignmentRecordId.Value,
                                    jobRequest.StudentId);
                        }
                        else
                        {
                                // Only generate instances for non-reassignment acceptances
                                await _GenerateJobInstancesAsync(jobRequest);
                        }


                        await _statusMaintenanceService.MaintainOrderStatuses(jobRequest.OrderId);
                }

                return _mapper.Map<JobRequestDto>(jobRequest);
        }

        /// 
        /// ONLY USE FOR FIRST ASIGNMENT ON A SCHEDULE
        /// Generated the first batch of JobInstances,
        /// the rest will be done by Hangfire recuring job
        public async Task _GenerateJobInstancesAsync(JobRequest jobRequest)
        {

                if (!jobRequest.OrderSchedule.Assignments.Any())
                {
                        _logger.LogInformation("❌ [JobRequest] -> [OrderSchedule] has no assignments");
                        return;
                }

                var pricingConfig = await _pricingConfigRepo.GetByIdAsync(1);

                if (pricingConfig == null)
                {
                        _logger.LogInformation("❌ Not pricing configuration found");
                        return;
                }

                /// this is okay because there is only 1 assignment
                var assignment = jobRequest.OrderSchedule.Assignments.First();

                var jobInstances = _recurringJobService.GenerateInstancesForAssignment(assignment, pricingConfig);

                await _jobInstanceRepository.AddRangeAsync(jobInstances);

        }


}