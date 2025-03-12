
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;


public class StudentService
{
        private readonly IStudentRepository _repository;
        private readonly IMapper _mapper;

        public StudentService(IStudentRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<StudentDto>> GetAllStudentsAsync()
        {
                var students = await _repository.GetAllStudentsAsync();
                return _mapper.Map<List<StudentDto>>(students);
        }

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
                var student = await _repository.GetByIdAsync(id);
                return _mapper.Map<StudentDto>(student);
        }


        public async Task<StudentDto> CreateStudentAsync(StudentCreateDto dto)
        {
                var student = _mapper.Map<Student>(dto);
                await _repository.AddAsync(student);
                return _mapper.Map<StudentDto>(student);
        }

        public async Task UpdateVerificationStatusAsync(int id, VerificationStatus status)
        {
                var student = await _repository.GetByIdAsync(id);
                student.VerificationStatus = status;
                await _repository.UpdateAsync(student);
        }
}