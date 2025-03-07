
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class StudentContractService
{
        private readonly IStudentContractRepository _repository;
        private readonly IMapper _mapper;

        public StudentContractService(IStudentContractRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<StudentContractDto>> GetContractsByStudentAsync(int studentId) =>
            _mapper.Map<List<StudentContractDto>>(await _repository.GetByStudentIdAsync(studentId));

        public async Task<StudentContractDto> CreateContractAsync(StudentContractCreateDto dto)
        {
                var contract = _mapper.Map<StudentContract>(dto);
                await _repository.AddAsync(contract);
                return _mapper.Map<StudentContractDto>(contract);
        }
}