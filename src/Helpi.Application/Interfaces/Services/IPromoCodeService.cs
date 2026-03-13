using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IPromoCodeService
{
    Task<List<PromoCodeDto>> GetAllAsync();
    Task<PromoCodeDto?> GetByIdAsync(int id);
    Task<PromoCodeDto> CreateAsync(PromoCodeCreateDto dto);
    Task<PromoCodeDto> UpdateAsync(int id, PromoCodeUpdateDto dto);
    Task DeleteAsync(int id);
    Task<PromoCodeValidationResultDto> ValidateCodeAsync(string code, int customerId, decimal orderTotal);
    Task<PromoCodeUsageDto> ApplyCodeAsync(string code, int orderId, int customerId, decimal orderTotal);
}
