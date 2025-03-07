
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ScheduleAssignmentReplacementService
{
        private readonly IScheduleAssignmentReplacementRepository _repository;
        private readonly IMapper _mapper;

        public ScheduleAssignmentReplacementService(IScheduleAssignmentReplacementRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        // public async Task<List<ScheduleAssignmentReplacementDto>> GetReplacementsByOriginalAsync(int originalId) =>
        //     _mapper.Map<List<ScheduleAssignmentReplacementDto>>(await _repository.GetByOriginalAsync(originalId));

        public async Task<ScheduleAssignmentReplacementDto> CreateReplacementAsync(ScheduleAssignmentReplacementCreateDto dto)
        {
                var replacement = _mapper.Map<ScheduleAssignmentReplacement>(dto);
                await _repository.AddAsync(replacement);
                return _mapper.Map<ScheduleAssignmentReplacementDto>(replacement);
        }
}