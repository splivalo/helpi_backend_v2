using System.Net.Http.Headers;
using System.Text;
using Helpi.Application.DTOs.Auth;
using Helpi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Helpi.Infrastructure.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(100);
        _logger = logger;
    }

    public async Task<TokenResponseDto> AuthenticateAsync(string url, string clientId, string clientSecret, string username, string password)
    {
        _logger.LogInformation("🔐 Authenticating user '{Username}'...", username);

        var postData = new StringBuilder();
        postData.Append($"client_id={clientId}&client_secret={clientSecret}&grant_type=password&");
        postData.Append($"username={username}&password={password}&scope=minimax.si");

        var content = new StringContent(postData.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(url, content);
        var json = await response.Content.ReadAsStringAsync();

        // _logger.LogInformation("🔐 Response from auth: {Response}", json);

        response.EnsureSuccessStatusCode();

        var jObject = JObject.Parse(json);
        var expiresInSeconds = jObject["expires_in"]?.Value<int>() ?? 0;

        var token = new TokenResponseDto
        {
            AccessToken = jObject["access_token"]?.ToString() ?? "",
            RefreshToken = jObject["refresh_token"]?.ToString() ?? "",
            AccessTokenExpiry = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            RefreshTokenExpiry = DateTime.UtcNow.AddSeconds(expiresInSeconds)
        };

        _logger.LogInformation("✅ Token acquired! Expires in {Expires}", token.AccessTokenExpiry);

        return token;
    }

    public async Task<JObject> GetAsync(string url, string accessToken)
    {
        _logger.LogInformation("📡 GET {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📥 Response: {Content}", content);
        response.EnsureSuccessStatusCode();

        return JObject.Parse(content);
    }

    public async Task<string> GetRawAsync(string url, string accessToken)
    {
        _logger.LogInformation("📡 GET (raw) {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📥 Raw Response: {Content}", content);
        response.EnsureSuccessStatusCode();

        return content;
    }

    public async Task<JObject> PostAsync(string url, string accessToken, string json)
    {
        _logger.LogInformation("📨 POST to {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📥 POST Response: {Content}", responseContent);
        response.EnsureSuccessStatusCode();

        return JObject.Parse(responseContent);
    }

    public async Task<string> PostRawAsync(string url, string accessToken, string json)
    {
        _logger.LogInformation("📨 POST RAW to {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📥 POST Raw Response: {Content}", responseContent);
        response.EnsureSuccessStatusCode();

        return responseContent;
    }

    public async Task<JObject> PutAsync(string url, string accessToken, string json)
    {
        _logger.LogInformation("✏️ PUT to {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📥 PUT Response: {Content}", responseContent);
        response.EnsureSuccessStatusCode();

        return JObject.Parse(responseContent);
    }

    public async Task DeleteAsync(string url, string accessToken)
    {
        _logger.LogWarning("🗑️ DELETE {Url}", url);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("❌ DELETE failed: {Error}", error);
        }
        else
        {
            _logger.LogInformation("🗑️ DELETE succeeded");
        }

        response.EnsureSuccessStatusCode();
    }
}
