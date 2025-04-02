using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Auth
{
    public class AdminRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();

    }
}
