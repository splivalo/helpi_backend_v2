
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class JobInstanceService
{
        private readonly IJobInstanceRepository _repository;
        private readonly IMapper _mapper;

        public JobInstanceService(IJobInstanceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<JobInstanceDto>> GetJobInstancesByAssignmentAsync(int assignmentId) =>
            _mapper.Map<List<JobInstanceDto>>(await _repository.GetByAssignmentAsync(assignmentId));

        //     public async Task<JobInstanceDto> CreateJobInstanceAsync(JobInstanceCreateDto dto)
        //     {
        //         var instance = _mapper.Map<JobInstance>(dto);
        //         await _repository.AddAsync(instance);
        //         return _mapper.Map<JobInstanceDto>(instance);
        //     }
}