namespace Helpi.Domain.Entities
{
    public class PromoCodeUsage
    {
        public int Id { get; set; }
        public int PromoCodeId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public PromoCode PromoCode { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }
}
