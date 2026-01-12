
using Helpi.Application.DTOs.Auth;
using Newtonsoft.Json.Linq;

namespace Helpi.Application.Interfaces;

public interface IApiService
{
    Task<TokenResponseDto> AuthenticateAsync(string url, string clientId, string clientSecret, string username, string password);
    Task<JObject> GetAsync(string url, string accessToken);
    Task<string> GetRawAsync(string url, string accessToken);
    Task<JObject> PostAsync(string url, string accessToken, string json);
    Task<string> PostRawAsync(string url, string accessToken, string json);
    Task<string> MinimaxPostRawAsync(string url, string accessToken, string json);
    Task<JObject> PutAsync(string url, string accessToken, string json);
    Task DeleteAsync(string url, string accessToken);

    Task<string> PostMultipartAsync(string url, string? accessToken = null, string? apiKey = null, MultipartFormDataContent? form = null);
}
