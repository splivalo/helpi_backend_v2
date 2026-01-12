using AutoMapper;
using Helpi.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class AdminService
{
    private readonly IAdminRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IAdminRepository repository, IMapper mapper, ILogger<AdminService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AdminDto?> GetAdminByIdAsync(int id)
    {
        var admin = await _repository.GetAdminByIdAsync(id);
        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {Id} not found", id);
            return null;
        }

        return _mapper.Map<AdminDto>(admin);
    }
}

