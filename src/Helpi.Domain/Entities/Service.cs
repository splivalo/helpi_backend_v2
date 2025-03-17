using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helpi.Domain.Entities
{

    public class Service
    {
        public int Id { get; set; }

        [ForeignKey(nameof(ServiceCategory))]
        public int CategoryId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = null!;


        public ServiceCategory Category { get; set; } = null!;
        // public ICollection<StudentService> StudentServices { get; set; } = new List<StudentService>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
