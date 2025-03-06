using System.ComponentModel.DataAnnotations;

namespace Helpi.Domain.Entities
{

    public class City
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string GooglePlaceId { get; set; } = null!;

        [MaxLength(255)]
        public string OfficialName { get; set; } = null!;

        public NetTopologySuite.Geometries.Polygon Bounds { get; set; } = null!;
        public bool IsServiced { get; set; } = false;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
        public ICollection<ServiceRegion> ServiceRegions { get; set; } = new List<ServiceRegion>();
    }
}
