using FirebaseAdmin.Auth;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.Services;



public class FirebaseService : IFirebaseService
{

    public async Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, object>? claims)
    {
        return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(userId, claims);
    }


}