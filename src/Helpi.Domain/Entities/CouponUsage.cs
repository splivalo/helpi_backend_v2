namespace Helpi.Domain.Entities
{
    public class CouponUsage
    {
        public int Id { get; set; }
        public int CouponAssignmentId { get; set; }
        public int JobInstanceId { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal? CoveredHours { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public CouponAssignment CouponAssignment { get; set; } = null!;
        public JobInstance JobInstance { get; set; } = null!;
    }
}
