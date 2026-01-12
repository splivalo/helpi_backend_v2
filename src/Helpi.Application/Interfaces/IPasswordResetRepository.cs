
namespace Helpi.Application.Interfaces;

public interface IPasswordResetRepository
{
    Task AddAsync(PasswordResetCode code);
    Task<PasswordResetCode?> GetValidCodeAsync(string email, string code);
    Task MarkAsUsedAsync(PasswordResetCode codeEntry);
}

