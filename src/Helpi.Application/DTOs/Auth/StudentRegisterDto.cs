using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.Auth
{
    public class StudentRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        // ContactInfo
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string GooglePlaceId { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public int CityId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Hr";


        public string? StudentNumber { get; set; }
        public int? FacultyId { get; set; }


    }
}
