namespace Helpi.Domain.Entities
{

    public class ServiceRegion
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public int ServiceId { get; set; }
        public bool Active { get; set; } = true;
        public int CoverageRadiusKm { get; set; }

        public City City { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}
