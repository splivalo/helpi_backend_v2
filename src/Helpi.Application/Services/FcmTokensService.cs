
using AutoMapper;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;
public class FcmTokensService
{
    private readonly IFcmTokensRepository _repository;
    private readonly IMapper _mapper;

    public FcmTokensService(IFcmTokensRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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


}