
namespace Helpi.Application.Interfaces.Services;
public interface IFirebaseService
{
    public Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, dynamic>? claims);
}