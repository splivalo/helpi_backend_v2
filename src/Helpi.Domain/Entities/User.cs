using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class User
    {
        public int Id { get; set; }
        public UserType UserType { get; set; }

        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Student? Student { get; set; }
        public Customer? Customer { get; set; }
    }
}
