
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;


public class ServiceCategoryService
{
        private readonly IServiceCategoryRepository _repository;
        private readonly IMapper _mapper;

        public ServiceCategoryService(IServiceCategoryRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<ServiceCategoryDto>> GetAllCategoriesAsync() =>
            _mapper.Map<List<ServiceCategoryDto>>(await _repository.GetAllAsync(excludeDeleted: true));

        public async Task<ServiceCategoryDto> CreateCategoryAsync(ServiceCategoryCreateDto dto)
        {
                var category = _mapper.Map<ServiceCategory>(dto);
                await _repository.AddAsync(category);
                return _mapper.Map<ServiceCategoryDto>(category);
        }

        public async Task<ServiceCategoryDto?> UpdateCategoryAsync(int id, ServiceCategoryUpdateDto dto)
        {
                var category = await _repository.GetByIdAsync(id);
                if (category == null)
                        return null;

                _mapper.Map(dto, category);

                await _repository.UpdateAsync(category);

                return _mapper.Map<ServiceCategoryDto>(category);
        }

}