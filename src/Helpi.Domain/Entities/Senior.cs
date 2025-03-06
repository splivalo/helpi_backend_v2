using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class Senior
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ContactId { get; set; }
        public Relationship Relationship { get; set; }

        public Customer Customer { get; set; } = null!;
        public ContactInfo Contact { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
