
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ScheduleAssignmentService
{
        private readonly IScheduleAssignmentRepository _repository;
        private readonly IMapper _mapper;

        public ScheduleAssignmentService(IScheduleAssignmentRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<ScheduleAssignmentDto>> GetAssignmentsByStudentAsync(int studentId)
        {
                var sa = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<ScheduleAssignmentDto>>(sa);
        }

        public async Task<ScheduleAssignmentDto> CreateAssignmentAsync(ScheduleAssignmentCreateDto dto)
        {
                var assignment = _mapper.Map<ScheduleAssignment>(dto);
                await _repository.AddAsync(assignment);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        public async Task<StudentDto> GetActiveStudentForOrderScheduleAsync(int orderScheduleId)
        {
                var student = await _repository.GetActiveStudentForOrderScheduleAsync(orderScheduleId);
                return _mapper.Map<StudentDto>(student);
        }

}