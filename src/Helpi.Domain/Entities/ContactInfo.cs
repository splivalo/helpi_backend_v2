using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class ContactInfo
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string FullName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }


        [MaxLength(20)]
        public string Phone { get; set; } = null!;
        public Gender Gender { get; set; }

        [MaxLength(255)]
        public string GooglePlaceId { get; set; } = null!;

        public string FullAddress { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int CityId { get; set; }

        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(2)]
        public string Country { get; set; } = "US";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public City City { get; set; } = null!;

        public Admin Admin { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public Senior Senior { get; set; } = null!;
    }
}
