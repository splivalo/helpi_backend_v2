using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Helpi.Domain.Entities
{

    public class User : IdentityUser<int>
    {

        public UserType UserType { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Student? Student { get; set; }
        public Customer? Customer { get; set; }

    }
}
