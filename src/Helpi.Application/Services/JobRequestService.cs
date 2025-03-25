
using AutoMapper;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class JobRequestService
{
        private readonly IJobRequestRepository _jobRequestRepository;
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IMapper _mapper;

        private readonly IRecurrenceDateGenerator _dateGenerator;

        public JobRequestService(IJobRequestRepository jobRequestRepository, IJobInstanceRepository jobInstanceRepository, IMapper mapper, IRecurrenceDateGenerator dateGenerator)
        {
                _jobRequestRepository = jobRequestRepository;
                _jobInstanceRepository = jobInstanceRepository;
                _mapper = mapper;
                _dateGenerator = dateGenerator;
        }

        public async Task<List<JobRequestDto>> GetStudentPendingRequests(int studentId)
        {

                var jobRequestsDtos = await _jobRequestRepository.GetStudentPendingRequests(studentId);
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
                }

                return _mapper.Map<JobRequestDto>(jobRequest);
        }

        /// 
        /// Generated the first batch of JobInstances,
        /// the rest will be done by Hangfire recuring job
        public async Task _GenerateJobInstancesAsync(JobRequest jobRequest)
        {

                var order = jobRequest.OrderSchedule.Order;

                DayOfWeek dayOfWeek = (DayOfWeek)jobRequest.OrderSchedule.DayOfWeek;

                var endDate = order.IsRecurring ? order.EndDate : order.StartDate;

                var dates = _dateGenerator.GetDates(
                        order.StartDate,
                        endDate,
                        order.RecurrencePattern ?? RecurrencePattern.Daily,
                        dayOfWeek,
                        3
                        );

                var jobInstances = new List<JobInstance>();

                var assignment = jobRequest.OrderSchedule.Assignments.First();

                foreach (var date in dates)
                {
                        jobInstances.Add(new JobInstance
                        {
                                ScheduleAssignmentId = assignment.Id,
                                ScheduledDate = date,
                                StartTime = jobRequest.OrderSchedule.StartTime,
                                EndTime = jobRequest.OrderSchedule.EndTime,
                                Status = JobInstanceStatus.Upcoming
                        });
                }


                await _jobInstanceRepository.AddRangeAsync(jobInstances);

        }

}