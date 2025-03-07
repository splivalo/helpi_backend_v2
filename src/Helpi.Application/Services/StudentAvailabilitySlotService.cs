
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class StudentAvailabilitySlotService
{
        private readonly IStudentAvailabilitySlotRepository _repository;
        private readonly IMapper _mapper;

        public StudentAvailabilitySlotService(IStudentAvailabilitySlotRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<StudentAvailabilitySlotDto>> GetSlotsByStudentAsync(int studentId) =>
            _mapper.Map<List<StudentAvailabilitySlotDto>>(await _repository.GetByStudentAsync(studentId));

        public async Task<StudentAvailabilitySlotDto> CreateSlotAsync(StudentAvailabilitySlotCreateDto dto)
        {
                var slot = _mapper.Map<StudentAvailabilitySlot>(dto);
                await _repository.AddAsync(slot);
                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }
}