
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
        private readonly CompletionStatusService _completionStatusService;
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobRequestService> _logger;

        private readonly IHangfireRecurringJobService _recurringJobService;
        private readonly IPricingConfigurationRepository _pricingConfigRepo;

        public JobRequestService(
                IJobRequestRepository jobRequestRepository,
                CompletionStatusService completionStatusService,
                IJobInstanceRepository jobInstanceRepository,
                IMapper mapper,
                 IHangfireRecurringJobService recurringJobService,
                 IPricingConfigurationRepository pricingConfigRepo,
                  ILogger<JobRequestService> logger)
        {
                _jobRequestRepository = jobRequestRepository;
                _completionStatusService = completionStatusService;
                _jobInstanceRepository = jobInstanceRepository;
                _mapper = mapper;
                _recurringJobService = recurringJobService;
                _pricingConfigRepo = pricingConfigRepo;
                _logger = logger;
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
                        await _GenerateJobInstancesAsync(jobRequest);

                        await _completionStatusService.ProcessIsOrderAllSchedulesAssigned(jobRequest.OrderId);
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