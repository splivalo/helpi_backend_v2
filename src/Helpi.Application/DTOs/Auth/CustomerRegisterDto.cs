using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Auth
{
    public class CustomerRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        // customer and (senior)
        public Relationship Relationship { get; set; } = Relationship.Self;
        public NotificationMethod PreferredNotificationMethod { get; set; } = NotificationMethod.Email;

        /// senior details
            // ContactInfo
        public ContactInfoCreateDto ContactInfo { get; set; } = new ContactInfoCreateDto();

        // Senior details
        public ContactInfoCreateDto? SeniorContactInfo { get; set; } = new ContactInfoCreateDto();
    }
}
