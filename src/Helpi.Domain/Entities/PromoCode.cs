using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public PromoCodeType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PromoCodeUsage> Usages { get; set; } = new List<PromoCodeUsage>();
    }
}
