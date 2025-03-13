
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;



public class StudentServiceService
{
        private readonly IStudentServiceRepository _repository;
        private readonly IMapper _mapper;

        public StudentServiceService(IStudentServiceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<StudentServiceDto> AddServiceToStudentAsync(StudentServiceCreateDto dto)
        {
                var studentService = _mapper.Map<StudentService>(dto);
                await _repository.AddAsync(studentService);
                return _mapper.Map<StudentServiceDto>(studentService);
        }

        public async Task<List<StudentServiceDto>> GetByStudentAsync(int studentId)
        {
                var studentServices = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<StudentServiceDto>>(studentServices);
        }

        public async Task RemoveServiceFromStudentAsync(int studentId, int serviceId)
        {
                await _repository.DeleteAsync(studentId, serviceId);
        }
}