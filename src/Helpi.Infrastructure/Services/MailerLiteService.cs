
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Infrastructure.Utilities;
using Microsoft.Extensions.Configuration;

namespace Helpi.Infrastructure.Services;

// TODO: PRODUCTION - Replace DUMMY MailerLite API key with real one
// Current: dummy key that won't work
// Need: API key from MailerLite dashboard
// Email groups/segments should be set up in MailerLite for welcome emails and contract notifications
public class MailerLiteService : IMailerLiteService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;



    public MailerLiteService(HttpClient httpClient, IConfiguration configuration)
    {

        var handler = new HttpClientHandler
        {
            // Disable connection pooling
            UseProxy = false,
            UseDefaultCredentials = false,
            AllowAutoRedirect = false,
        };

        _httpClient = new HttpClient(handler);
        _configuration = configuration;


        var creds = CredentialLoader.Load(_configuration, "MailerLite");
        var _apiKey = creds.GetString("ApiKey")!;





        _httpClient.BaseAddress = new Uri("https://connect.mailerlite.com/api/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.Timeout = TimeSpan.FromSeconds(100);
    }

    public async Task<bool> AddSubscriberAsync(MailerLiteSubscriberDto subscriber)
    {
        try
        {

            var creds = CredentialLoader.Load(_configuration, "MailerLite");
            var groupId = creds.GetString($"Groups:{subscriber.Group}")!;

            var body = new
            {
                email = subscriber.Email,
                fields = new { name = subscriber.Name },
                groups = new[] { groupId },
                status = "active"
            };


            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "subscribers")
            {
                Content = content
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            // Console.WriteLine($"Request URL: {_httpClient.BaseAddress}{request.RequestUri}");
            // Console.WriteLine($"Request Body: {json}");
            // Console.WriteLine($"Authorization: {_httpClient.DefaultRequestHeaders.Authorization}");
            // Console.WriteLine($"Accept: {request.Headers}");
            // Console.WriteLine($"Timeout: {_httpClient.Timeout}");


            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var response = await _httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"MailerLite API error: {error}");
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"-->  MailerLite API error: {e}");
            return false;
        }
    }
}

