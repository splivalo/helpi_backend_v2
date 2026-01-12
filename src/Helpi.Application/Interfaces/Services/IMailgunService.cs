using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helpi.Application.Interfaces.Services
{
    public interface IMailgunService
    {
        Task<bool> SendEmailAsync(
            string to,
            string subject,
            string htmlBody,
            string? textBody = null,
            Dictionary<string, string>? attachments = null // key = filename, value = base64 data
        );
    }
}
