
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;

public interface IFirebaseService
{
    public Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, dynamic>? claims);

    public Task<bool> SendPushNotificationAsync(List<FcmToken> deviceTokens, HNotificationDto notification);

    /// <summary>
    /// Deletes the user's Firestore document and revokes their Firebase refresh tokens.
    /// This effectively logs the user out and removes their Firebase data.
    /// </summary>
    /// <param name="backendUserId">The backend user ID (which is used as Firebase UID)</param>
    public Task AnonymizeAndLogoutUserAsync(int backendUserId);
}