using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface ISuspensionLogRepository
{
    Task<List<SuspensionLog>> GetByUserIdAsync(int userId);
    Task<SuspensionLog> AddAsync(SuspensionLog log);
}
