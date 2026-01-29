
using AutoMapper;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;
public class FcmTokensService
{
    private readonly IFcmTokensRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<FcmTokensService> _logger;

    public FcmTokensService(IFcmTokensRepository repository, IMapper mapper, ILogger<FcmTokensService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FcmToken> CreateFcmTokenAsync(FcmToken dto)
    {

        await _repository.AddAsync(dto);
        return dto;
    }
    public async Task<List<FcmToken>> GetUserFcmTokens(int userId)
    {

        return await _repository.GetTokensByUserIdAsync(userId);

    }

    public async Task DeleteUserFcmTokensAsync(int userId)
    {
        _logger.LogInformation("🗑️ Deleting all FCM tokens for user {UserId}", userId);

        try
        {
            await _repository.DeleteByUserIdAsync(userId);
            _logger.LogInformation("✅ Successfully deleted FCM tokens for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete FCM tokens for user {UserId}", userId);
            throw;
        }
    }
}