
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class JobRequestService
{
        private readonly IJobRequestRepository _repository;
        private readonly IMapper _mapper;

        public JobRequestService(IJobRequestRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<JobRequestDto>> GetPendingRequestsAsync()
        {
                //  _mapper.Map<List<JobRequestDto>>(await _repository.GetPendingAsync());
                return null;
        }

        public async Task<JobRequestDto> CreateJobRequestAsync(JobRequestCreateDto dto)
        {
                var request = _mapper.Map<JobRequest>(dto);
                await _repository.AddAsync(request);
                return _mapper.Map<JobRequestDto>(request);
        }
}