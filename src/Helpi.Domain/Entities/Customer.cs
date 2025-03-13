using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class Customer
    {
        [Key]
        public int UserId { get; set; } // user id
        public int ContactId { get; set; }
        public NotificationMethod PreferredNotificationMethod { get; set; } = NotificationMethod.Email;

        // public User User { get; set; } = null!;
        public ContactInfo Contact { get; set; } = null!;
        public ICollection<Senior> Seniors { get; set; } = new List<Senior>();
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}
