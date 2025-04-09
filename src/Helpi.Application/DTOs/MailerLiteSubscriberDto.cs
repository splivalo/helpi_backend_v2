
namespace Helpi.Application.DTOs;
public class MailerLiteSubscriberDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty; // "Customers", "Students",
}