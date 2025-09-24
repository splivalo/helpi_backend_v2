
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ServiceRegionService
{
        private readonly IServiceRegionRepository _repository;
        private readonly IMapper _mapper;

        public ServiceRegionService(IServiceRegionRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<ServiceRegionDto>> GetRegionsByServiceAsync(int serviceId) =>
            _mapper.Map<List<ServiceRegionDto>>(await _repository.GetByServiceAsync(serviceId));

        public async Task<ServiceRegionDto> CreateServiceRegionAsync(ServiceRegionCreateDto dto)
        {
                var region = _mapper.Map<ServiceRegion>(dto);
                await _repository.AddAsync(region);
                return _mapper.Map<ServiceRegionDto>(region);
        }
}