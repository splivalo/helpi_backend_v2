
using AutoMapper;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class JobRequestService
{
        private readonly IJobRequestRepository _jobRequestRepository;
        private readonly IMapper _mapper;

        public JobRequestService(IJobRequestRepository jobRequestRepository, IMapper mapper)
        {
                _jobRequestRepository = jobRequestRepository;
                _mapper = mapper;
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

                await _jobRequestRepository.RespondToJobRequestAsync(jobRequest);

                return _mapper.Map<JobRequestDto>(jobRequest);
        }

}