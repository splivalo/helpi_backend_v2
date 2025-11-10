using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> GetAdminByIdAsync(int id);
}

