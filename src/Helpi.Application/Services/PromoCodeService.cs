using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly IPromoCodeRepository _repo;
    private readonly IMapper _mapper;

    public PromoCodeService(IPromoCodeRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<PromoCodeDto>> GetAllAsync()
    {
        var codes = await _repo.GetAllAsync();
        return _mapper.Map<List<PromoCodeDto>>(codes);
    }

    public async Task<PromoCodeDto?> GetByIdAsync(int id)
    {
        var code = await _repo.GetByIdAsync(id);
        return code == null ? null : _mapper.Map<PromoCodeDto>(code);
    }

    public async Task<PromoCodeDto> CreateAsync(PromoCodeCreateDto dto)
    {
        var existing = await _repo.GetByCodeAsync(dto.Code.ToUpperInvariant());
        if (existing != null)
            throw new ArgumentException($"Promo code '{dto.Code}' already exists.");

        var entity = new PromoCode
        {
            Code = dto.Code.ToUpperInvariant(),
            Type = dto.Type,
            DiscountValue = dto.DiscountValue,
            MaxUses = dto.MaxUses,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            IsActive = true,
            CurrentUses = 0
        };

        if (entity.Type == PromoCodeType.Percentage && entity.DiscountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.");

        var created = await _repo.AddAsync(entity);
        return _mapper.Map<PromoCodeDto>(created);
    }

    public async Task<PromoCodeDto> UpdateAsync(int id, PromoCodeUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Promo code with ID {id} not found.");

        if (dto.DiscountValue.HasValue)
        {
            if (entity.Type == PromoCodeType.Percentage && dto.DiscountValue.Value > 100)
                throw new ArgumentException("Percentage discount cannot exceed 100%.");
            entity.DiscountValue = dto.DiscountValue.Value;
        }

        if (dto.MaxUses.HasValue) entity.MaxUses = dto.MaxUses.Value;
        if (dto.ValidFrom.HasValue) entity.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidUntil.HasValue) entity.ValidUntil = dto.ValidUntil.Value;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        await _repo.UpdateAsync(entity);
        return _mapper.Map<PromoCodeDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Promo code with ID {id} not found.");
        await _repo.DeleteAsync(entity);
    }

    public async Task<PromoCodeValidationResultDto> ValidateCodeAsync(string code, int customerId, decimal orderTotal)
    {
        var promo = await _repo.GetByCodeAsync(code.ToUpperInvariant());

        if (promo == null)
            return new PromoCodeValidationResultDto { IsValid = false, ErrorMessage = "Promo code not found." };

        var error = ValidatePromoCode(promo, customerId);
        if (error != null)
            return new PromoCodeValidationResultDto { IsValid = false, ErrorMessage = error };

        var alreadyUsed = await _repo.HasCustomerUsedCodeAsync(promo.Id, customerId);
        if (alreadyUsed)
            return new PromoCodeValidationResultDto { IsValid = false, ErrorMessage = "You have already used this promo code." };

        var discount = CalculateDiscount(promo, orderTotal);

        return new PromoCodeValidationResultDto
        {
            IsValid = true,
            PromoCode = _mapper.Map<PromoCodeDto>(promo),
            DiscountAmount = discount
        };
    }

    public async Task<PromoCodeUsageDto> ApplyCodeAsync(string code, int orderId, int customerId, decimal orderTotal)
    {
        var promo = await _repo.GetByCodeAsync(code.ToUpperInvariant())
            ?? throw new ArgumentException("Promo code not found.");

        var error = ValidatePromoCode(promo, customerId);
        if (error != null)
            throw new ArgumentException(error);

        var alreadyUsed = await _repo.HasCustomerUsedCodeAsync(promo.Id, customerId);
        if (alreadyUsed)
            throw new ArgumentException("You have already used this promo code.");

        var discount = CalculateDiscount(promo, orderTotal);

        var usage = new PromoCodeUsage
        {
            PromoCodeId = promo.Id,
            OrderId = orderId,
            CustomerId = customerId,
            DiscountApplied = discount
        };

        promo.CurrentUses++;
        await _repo.UpdateAsync(promo);
        var created = await _repo.AddUsageAsync(usage);

        return new PromoCodeUsageDto
        {
            Id = created.Id,
            PromoCodeId = promo.Id,
            PromoCodeCode = promo.Code,
            OrderId = orderId,
            CustomerId = customerId,
            DiscountApplied = discount,
            UsedAt = created.UsedAt
        };
    }

    private static string? ValidatePromoCode(PromoCode promo, int customerId)
    {
        if (!promo.IsActive)
            return "This promo code is no longer active.";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (promo.ValidFrom.HasValue && today < promo.ValidFrom.Value)
            return "This promo code is not yet valid.";

        if (promo.ValidUntil.HasValue && today > promo.ValidUntil.Value)
            return "This promo code has expired.";

        if (promo.MaxUses.HasValue && promo.CurrentUses >= promo.MaxUses.Value)
            return "This promo code has reached its maximum number of uses.";

        return null;
    }

    private static decimal CalculateDiscount(PromoCode promo, decimal orderTotal)
    {
        return promo.Type switch
        {
            PromoCodeType.Percentage => Math.Round(orderTotal * (promo.DiscountValue / 100), 2),
            PromoCodeType.FixedAmount => Math.Min(promo.DiscountValue, orderTotal),
            _ => 0
        };
    }
}
