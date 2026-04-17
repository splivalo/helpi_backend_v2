using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public CouponType Type { get; set; }
        public decimal Value { get; set; }
        public bool IsCombainable { get; set; }
        public int? CityId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidUntil { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public City? City { get; set; }
        public ICollection<CouponAssignment> Assignments { get; set; } = new List<CouponAssignment>();
    }
}
