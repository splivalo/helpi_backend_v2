using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IPromoCodeRepository
{
    Task<PromoCode?> GetByIdAsync(int id);
    Task<PromoCode?> GetByCodeAsync(string code);
    Task<IEnumerable<PromoCode>> GetAllAsync();
    Task<PromoCode> AddAsync(PromoCode promoCode);
    Task UpdateAsync(PromoCode promoCode);
    Task DeleteAsync(PromoCode promoCode);
    Task<bool> HasCustomerUsedCodeAsync(int promoCodeId, int customerId);
    Task<PromoCodeUsage> AddUsageAsync(PromoCodeUsage usage);
    Task<IEnumerable<PromoCodeUsage>> GetUsagesByPromoCodeIdAsync(int promoCodeId);
}
