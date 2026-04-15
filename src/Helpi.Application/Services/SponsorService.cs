using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Helpi.Application.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _repo;
    private readonly IMapper _mapper;

    public SponsorService(ISponsorRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<SponsorDto>> GetAllAsync()
    {
        var sponsors = await _repo.GetAllAsync();
        return _mapper.Map<List<SponsorDto>>(sponsors);
    }

    public async Task<List<SponsorDto>> GetActiveAsync()
    {
        var sponsors = await _repo.GetActiveAsync();
        return _mapper.Map<List<SponsorDto>>(sponsors);
    }

    public async Task<SponsorDto?> GetByIdAsync(int id)
    {
        var sponsor = await _repo.GetByIdAsync(id);
        return sponsor == null ? null : _mapper.Map<SponsorDto>(sponsor);
    }

    public async Task<SponsorDto> CreateAsync(SponsorCreateDto dto)
    {
        var entity = new Sponsor
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            DarkLogoUrl = dto.DarkLogoUrl,
            LinkUrl = dto.LinkUrl,
            Label = dto.Label ?? new Dictionary<string, string> { ["hr"] = "Uz podršku" },
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
        };

        var created = await _repo.AddAsync(entity);
        return _mapper.Map<SponsorDto>(created);
    }

    public async Task<SponsorDto> UpdateAsync(int id, SponsorUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Sponsor with ID {id} not found.");

        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.LogoUrl != null) entity.LogoUrl = dto.LogoUrl;
        if (dto.DarkLogoUrl != null) entity.DarkLogoUrl = dto.DarkLogoUrl;
        if (dto.LinkUrl != null) entity.LinkUrl = dto.LinkUrl;
        if (dto.Label != null)
        {
            // Merge incoming translations with existing ones
            foreach (var kvp in dto.Label)
            {
                entity.Label[kvp.Key] = kvp.Value;
            }
        }
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue) entity.DisplayOrder = dto.DisplayOrder.Value;

        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity);
        return _mapper.Map<SponsorDto>(entity);
    }

    public async Task<string> UploadLogoAsync(int id, IFormFile file, string variant, string webRootPath)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Sponsor with ID {id} not found.");

        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 5 MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new HashSet<string> { ".svg", ".png", ".jpg", ".jpeg", ".webp" };
        if (!allowed.Contains(ext))
            throw new ArgumentException($"File type '{ext}' is not allowed. Use svg, png, jpg, jpeg, or webp.");

        // Determine which field to update
        var isDark = string.Equals(variant, "dark", StringComparison.OrdinalIgnoreCase);
        var existingUrl = isDark ? entity.DarkLogoUrl : entity.LogoUrl;

        // Delete old file if exists
        if (!string.IsNullOrEmpty(existingUrl))
        {
            var oldPath = Path.Combine(webRootPath, existingUrl.TrimStart('/'));
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        var uploadsDir = Path.Combine(webRootPath, "uploads", "sponsor-logos");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"sponsor_{id}_{variant}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var relativeUrl = $"/uploads/sponsor-logos/{fileName}";

        if (isDark)
            entity.DarkLogoUrl = relativeUrl;
        else
            entity.LogoUrl = relativeUrl;

        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);

        return relativeUrl;
    }

    public async Task DeleteLogoAsync(int id, string variant, string webRootPath)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Sponsor with ID {id} not found.");

        var isDark = string.Equals(variant, "dark", StringComparison.OrdinalIgnoreCase);
        var existingUrl = isDark ? entity.DarkLogoUrl : entity.LogoUrl;

        if (string.IsNullOrEmpty(existingUrl))
            return;

        // Delete file from disk
        var filePath = Path.Combine(webRootPath, existingUrl.TrimStart('/'));
        if (File.Exists(filePath))
            File.Delete(filePath);

        // Clear URL in DB
        if (isDark)
            entity.DarkLogoUrl = null;
        else
            entity.LogoUrl = string.Empty;

        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Sponsor with ID {id} not found.");
        await _repo.DeleteAsync(entity);
    }
}
