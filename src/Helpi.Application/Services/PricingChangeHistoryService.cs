using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;

namespace Helpi.Application.Services;

public class PricingChangeHistoryService
{
    private readonly IPricingChangeHistoryRepository _repository;
    private readonly IMapper _mapper;
    public PricingChangeHistoryService(
        IPricingChangeHistoryRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }



    public async Task<IEnumerable<PricingChangeHistoryDto>> GetAllAsync()
    {
        var histories = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<PricingChangeHistoryDto>>(histories);
    }

    public async Task<IEnumerable<PricingChangeHistoryDto>> GetByConfigurationIdAsync(int configId)
    {
        var histories = await _repository.GetByConfigurationIdAsync(configId);
        return _mapper.Map<IEnumerable<PricingChangeHistoryDto>>(histories);
    }
}
