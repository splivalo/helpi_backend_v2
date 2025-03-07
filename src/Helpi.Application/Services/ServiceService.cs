
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ServiceService
{
        private readonly IServiceRepository _repository;
        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<ServiceDto>> GetServicesByCategoryAsync(int categoryId) =>
            _mapper.Map<List<ServiceDto>>(await _repository.GetByCategoryAsync(categoryId));

        public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto dto)
        {
                var service = _mapper.Map<Service>(dto);
                await _repository.AddAsync(service);
                return _mapper.Map<ServiceDto>(service);
        }
}