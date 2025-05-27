

using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class PricingConfigurationService
{
    private readonly IPricingConfigurationRepository _configRepo;
    private readonly IPricingChangeHistoryRepository _historyRepo;

    public PricingConfigurationService(

        IPricingConfigurationRepository configRepo,
         IPricingChangeHistoryRepository historyRepo)
    {
        _configRepo = configRepo;
        _historyRepo = historyRepo;
    }

    public async Task<IEnumerable<PricingConfigurationDto>> GetAllConfigurationsAsync()
    {
        var configs = await _configRepo.GetAllAsync();
        return configs.Select(c => new PricingConfigurationDto
        {
            Id = c.Id,
            JobHourlyRate = c.JobHourlyRate,
            CompanyPercentage = c.CompanyPercentage,
            ServiceProviderPercentage = c.ServiceProviderPercentage
        });
    }

    public async Task<PricingConfigurationDto?> GetConfigurationByIdAsync(int id)
    {
        var config = await _configRepo.GetByIdAsync(id);
        if (config == null) return null;

        return new PricingConfigurationDto
        {
            Id = config.Id,
            JobHourlyRate = config.JobHourlyRate,
            CompanyPercentage = config.CompanyPercentage,
            ServiceProviderPercentage = config.ServiceProviderPercentage
        };
    }

    public async Task AddConfigurationAsync(PricingConfigurationDto configDto)
    {

        var existing = await _configRepo.GetAllAsync();
        if (existing.Any())
            throw new InvalidOperationException("Only one PricingConfiguration is allowed.");

        var config = new PricingConfiguration
        {
            JobHourlyRate = configDto.JobHourlyRate,
            CompanyPercentage = configDto.CompanyPercentage,
            ServiceProviderPercentage = configDto.ServiceProviderPercentage
        };

        await _configRepo.AddAsync(config);
    }

    public async Task UpdateConfigurationAsync(PricingConfigurationDto configDto, int changedBy, string reason)
    {
        var existingConfig = await _configRepo.GetByIdAsync(configDto.Id);
        if (existingConfig == null) throw new Exception("Configuration not found.");

        var history = new PricingChangeHistory
        {
            PricingConfigurationId = existingConfig.Id,
            OldJobHourlyRate = existingConfig.JobHourlyRate,
            OldCompanyPercentage = existingConfig.CompanyPercentage,
            OldServiceProviderPercentage = existingConfig.ServiceProviderPercentage,
            NewJobHourlyRate = configDto.JobHourlyRate,
            NewCompanyPercentage = configDto.CompanyPercentage,
            NewServiceProviderPercentage = configDto.ServiceProviderPercentage,
            ChangeDate = DateTime.UtcNow,
            ChangedBy = changedBy,
            Reason = reason
        };

        existingConfig.JobHourlyRate = configDto.JobHourlyRate;
        existingConfig.CompanyPercentage = configDto.CompanyPercentage;
        existingConfig.ServiceProviderPercentage = configDto.ServiceProviderPercentage;

        await _configRepo.UpdateAsync(existingConfig);
        await _historyRepo.AddAsync(history);
    }

    public async Task DeleteConfigurationAsync(int id)
    {
        await _configRepo.DeleteAsync(id);
    }
}
