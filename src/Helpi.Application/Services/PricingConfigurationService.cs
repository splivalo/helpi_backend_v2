

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
            SundayHourlyRate = c.SundayHourlyRate,
            CompanyPercentage = c.CompanyPercentage,
            ServiceProviderPercentage = c.ServiceProviderPercentage,
            StudentCancelCutoffHours = c.StudentCancelCutoffHours,
            SeniorCancelCutoffHours = c.SeniorCancelCutoffHours,
            TravelBufferMinutes = c.TravelBufferMinutes,
            PaymentTimingMinutes = c.PaymentTimingMinutes,
            VatEnabled = c.VatEnabled,
            VatPercentage = c.VatPercentage
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
            SundayHourlyRate = config.SundayHourlyRate,
            CompanyPercentage = config.CompanyPercentage,
            ServiceProviderPercentage = config.ServiceProviderPercentage,
            StudentCancelCutoffHours = config.StudentCancelCutoffHours,
            SeniorCancelCutoffHours = config.SeniorCancelCutoffHours,
            TravelBufferMinutes = config.TravelBufferMinutes,
            PaymentTimingMinutes = config.PaymentTimingMinutes,
            VatEnabled = config.VatEnabled,
            VatPercentage = config.VatPercentage
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
            SundayHourlyRate = configDto.SundayHourlyRate,
            CompanyPercentage = configDto.CompanyPercentage,
            ServiceProviderPercentage = configDto.ServiceProviderPercentage,
            StudentCancelCutoffHours = configDto.StudentCancelCutoffHours,
            SeniorCancelCutoffHours = configDto.SeniorCancelCutoffHours,
            TravelBufferMinutes = configDto.TravelBufferMinutes,
            PaymentTimingMinutes = configDto.PaymentTimingMinutes,
            VatEnabled = configDto.VatEnabled,
            VatPercentage = configDto.VatPercentage
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
            OldSundayHourlyRate = existingConfig.SundayHourlyRate,
            OldCompanyPercentage = existingConfig.CompanyPercentage,
            OldServiceProviderPercentage = existingConfig.ServiceProviderPercentage,
            NewJobHourlyRate = configDto.JobHourlyRate,
            NewSundayHourlyRate = configDto.SundayHourlyRate,
            NewCompanyPercentage = configDto.CompanyPercentage,
            NewServiceProviderPercentage = configDto.ServiceProviderPercentage,
            ChangeDate = DateTime.UtcNow,
            ChangedBy = changedBy,
            Reason = reason
        };

        existingConfig.JobHourlyRate = configDto.JobHourlyRate;
        existingConfig.SundayHourlyRate = configDto.SundayHourlyRate;
        existingConfig.CompanyPercentage = configDto.CompanyPercentage;
        existingConfig.ServiceProviderPercentage = configDto.ServiceProviderPercentage;
        existingConfig.StudentCancelCutoffHours = configDto.StudentCancelCutoffHours;
        existingConfig.SeniorCancelCutoffHours = configDto.SeniorCancelCutoffHours;
        existingConfig.TravelBufferMinutes = configDto.TravelBufferMinutes;
        existingConfig.PaymentTimingMinutes = configDto.PaymentTimingMinutes;
        existingConfig.VatEnabled = configDto.VatEnabled;
        existingConfig.VatPercentage = configDto.VatPercentage;

        await _configRepo.UpdateAsync(existingConfig);
        await _historyRepo.AddAsync(history);
    }

    public async Task DeleteConfigurationAsync(int id)
    {
        await _configRepo.DeleteAsync(id);
    }
}
