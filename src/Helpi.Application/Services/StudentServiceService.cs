
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class StudentServiceService
{
        private readonly IStudentServiceRepository _repository;
        private readonly IMapper _mapper;

        public StudentServiceService(IStudentServiceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task AddServiceToStudentAsync(StudentServiceCreateDto dto)
        {
                // var studentService = _mapper.Map<StudentService>(dto);
                // await _repository.AddAsync(studentService);

        }

        public async Task RemoveServiceFromStudentAsync(int studentId, int serviceId) =>
            await _repository.DeleteAsync(studentId, serviceId);
}