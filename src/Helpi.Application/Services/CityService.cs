
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class CityService
{
        private readonly ICityRepository _repository;
        private readonly IMapper _mapper;

        public CityService(ICityRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<CityDto>> GetAllCitiesAsync()
        {
                var cities = await _repository.GetAllAsync();


                return _mapper.Map<List<CityDto>>(cities);
        }

        public async Task<CityDto> CreateCityAsync(CityCreateDto dto)
        {
                var city = _mapper.Map<City>(dto);
                await _repository.AddAsync(city);
                return _mapper.Map<CityDto>(city);
        }
}