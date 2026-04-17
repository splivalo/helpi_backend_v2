namespace Helpi.Domain.Entities
{
    public class CouponAssignment
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int SeniorId { get; set; }
        public int? AssignedByAdminId { get; set; }
        public decimal? RemainingValue { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public Coupon Coupon { get; set; } = null!;
        public Senior Senior { get; set; } = null!;
        public Admin? AssignedByAdmin { get; set; }
        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    }
}
