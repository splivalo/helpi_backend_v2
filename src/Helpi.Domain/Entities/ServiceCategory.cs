using System.ComponentModel.DataAnnotations;

namespace Helpi.Domain.Entities
{

    public class ServiceCategory
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [MaxLength(50)]
        public string? Icon { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
