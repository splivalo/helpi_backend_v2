
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;


public class FacultyService
{
        private readonly IFacultyRepository _repository;
        private readonly IMapper _mapper;

        public FacultyService(IFacultyRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<FacultyDto>> GetAllFacultiesAsync() =>
            _mapper.Map<List<FacultyDto>>(await _repository.GetAllAsync());

        public async Task<FacultyDto> CreateFacultyAsync(FacultyCreateDto dto)
        {
                var faculty = _mapper.Map<Faculty>(dto);
                await _repository.AddAsync(faculty);
                return _mapper.Map<FacultyDto>(faculty);
        }
}