
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Helpi.Infrastructure.Services;

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


        var apiKey = Environment.GetEnvironmentVariable("MailerLite:ApiKey")
                     ?? _configuration["MailerLite:ApiKey"]
                     ?? throw new ArgumentNullException("MailerLite:ApiKey");

        _httpClient.BaseAddress = new Uri("https://connect.mailerlite.com/api/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.Timeout = TimeSpan.FromSeconds(100);
    }

    public async Task<bool> AddSubscriberAsync(MailerLiteSubscriberDto subscriber)
    {
        try
        {
            var groupId = _configuration[$"MailerLite:Groups:{subscriber.Group}"];

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

