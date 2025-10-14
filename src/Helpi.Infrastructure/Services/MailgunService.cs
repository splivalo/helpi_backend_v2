using System.Net.Http.Headers;
using System.Text;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Helpi.Infrastructure.Utilities;
using Helpi.Application.Interfaces;

namespace Helpi.Infrastructure.Services
{
    public class MailgunService : IMailgunService
    {

        private readonly IConfiguration _configuration;
        private readonly IApiService _apiService;


        private readonly string _apiBaseUrl;
        private readonly string _apiKey;
        private readonly string _domain;

        public MailgunService(IApiService apiService, IConfiguration configuration)
        {

            _apiService = apiService;
            _configuration = configuration;

            var creds = CredentialLoader.Load(_configuration, "Mailgun");
            _apiKey = creds.GetString("ApiKey")!;
            _domain = creds.GetString("Domain")!;

            _apiBaseUrl = $"https://api.eu.mailgun.net/v3/{_domain}";

        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string htmlBody,
            string? textBody = null,
            Dictionary<string, string>? attachments = null)
        {
            try
            {
                using var form = new MultipartFormDataContent();

                form.Add(new StringContent("Helpi <no-reply@" + _domain + ">"), "from");
                form.Add(new StringContent(to), "to");
                form.Add(new StringContent(subject), "subject");
                form.Add(new StringContent(htmlBody), "html");

                if (!string.IsNullOrEmpty(textBody))
                    form.Add(new StringContent(textBody), "text");

                // Attachments (if any)
                if (attachments != null)
                {
                    foreach (var (fileName, base64Data) in attachments)
                    {
                        var fileBytes = Convert.FromBase64String(base64Data);
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                        form.Add(fileContent, "attachment", fileName);
                    }
                }

                var response = await _apiService.PostMultipartAsync(
                        url: $"{_apiBaseUrl}/messages",
                        apiKey: _apiKey,
                        form: form
                        );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MailgunService.SendEmailAsync failed: {ex.Message}");
                return false;
            }
        }
    }
}
