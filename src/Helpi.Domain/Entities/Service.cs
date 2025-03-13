using System.ComponentModel.DataAnnotations;

namespace Helpi.Domain.Entities
{

    public class Service
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = null!;

        // [Precision(10, 2)]
        public decimal BasePrice { get; set; }
        public short MinDuration { get; set; }

        public ServiceCategory Category { get; set; } = null!;
        // public ICollection<StudentService> StudentServices { get; set; } = new List<StudentService>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
