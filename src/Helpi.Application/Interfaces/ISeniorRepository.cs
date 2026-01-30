using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface ISeniorRepository
{
    Task<Senior?> GetByIdAsync(int id);
    Task<IEnumerable<Senior>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Senior>> GetByRelationshipAsync(Relationship relationship);
    Task<Senior> AddAsync(Senior senior);
    Task UpdateAsync(Senior senior);
    Task<List<Senior>> GetSeniorsAsync();
    Task<List<SeniorDto>> GetSeniorsWithExtraDetailsAsync(SeniorFilterDto? filter);
}
