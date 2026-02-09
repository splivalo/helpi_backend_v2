using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IFcmTokensRepository
{
    Task<List<FcmToken>> GetTokensByUserIdAsync(int userId);
    Task<FcmToken> AddAsync(FcmToken fcmToken);

    Task DeleteAsync(FcmToken fcmToken);
    Task DeleteByUserIdAsync(int userId);
}