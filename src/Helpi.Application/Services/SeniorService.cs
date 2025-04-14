
using System.Text.Json;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class SeniorService
{
        private readonly ISeniorRepository _repository;
        private readonly IMapper _mapper;

        public SeniorService(ISeniorRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<SeniorDto>> GetSeniorsByCustomerAsync(int customerId)
        {
                var seniors = await _repository.GetByCustomerIdAsync(customerId);

                return _mapper.Map<List<SeniorDto>>(seniors);
        }

        public async Task<SeniorDto> CreateSeniorAsync(SeniorCreateDto dto)
        {
                var senior = _mapper.Map<Senior>(dto);
                await _repository.AddAsync(senior);
                return _mapper.Map<SeniorDto>(senior);
        }

        public async Task<List<SeniorDto>> GetBySeniorsAsync()
        {
                var seniors = await _repository.GetBySeniorsAsync();

                return _mapper.Map<List<SeniorDto>>(seniors);
        }

}