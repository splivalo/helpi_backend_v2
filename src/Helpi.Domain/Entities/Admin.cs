using System.ComponentModel.DataAnnotations;


namespace Helpi.Domain.Entities
{
    public class Admin
    {
        [Key]
        public int UserId { get; set; }

        public int ContactId { get; set; }

        public ContactInfo Contact { get; set; } = null!;
    }
}
