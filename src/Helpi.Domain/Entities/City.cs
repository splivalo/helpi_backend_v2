using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace Helpi.Domain.Entities
{

    public class City
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public required string GooglePlaceId { get; set; }

        [MaxLength(255)]
        public required string Name { get; set; } = null!;
        public required string PostalCode { get; set; } = null!;

        public Polygon? Bounds { get; set; }
        public bool IsServiced { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
        public ICollection<ServiceRegion> ServiceRegions { get; set; } = new List<ServiceRegion>();
    }
}
