using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Auth
{
    public class CustomerRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }


        public NotificationMethod PreferredNotificationMethod { get; set; } = NotificationMethod.Email;

        /// senior details
            // ContactInfo
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();

        // Senior details
        public ContactInfoDto SeniorContactInfo { get; set; } = new ContactInfoDto();
    }
}
