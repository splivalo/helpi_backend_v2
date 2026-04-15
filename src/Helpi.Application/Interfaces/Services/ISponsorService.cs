using Helpi.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Helpi.Application.Interfaces.Services;

public interface ISponsorService
{
    Task<List<SponsorDto>> GetAllAsync();
    Task<List<SponsorDto>> GetActiveAsync();
    Task<SponsorDto?> GetByIdAsync(int id);
    Task<SponsorDto> CreateAsync(SponsorCreateDto dto);
    Task<SponsorDto> UpdateAsync(int id, SponsorUpdateDto dto);
    Task<string> UploadLogoAsync(int id, IFormFile file, string variant, string webRootPath);
    Task DeleteLogoAsync(int id, string variant, string webRootPath);
    Task DeleteAsync(int id);
}
