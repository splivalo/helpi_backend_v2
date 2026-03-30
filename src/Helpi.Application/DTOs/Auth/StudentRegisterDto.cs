using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Auth
{
    public class StudentRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public int? FacultyId { get; set; }

        // ContactInfo
        public ContactInfoCreateDto ContactInfo { get; set; } = new ContactInfoCreateDto();





    }
}
