
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

        public bool IsSuspended { get; set; } = false;

        [MaxLength(500)]
        public string? SuspensionReason { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public int? SuspendedByAdminId { get; set; }

        public Student? Student { get; set; }
        public Customer? Customer { get; set; }
        public Admin? Admin { get; set; }


        public ICollection<FcmToken> fcmTokens { get; set; } = new List<FcmToken>();

        public ICollection<PaymentProfile> PaymentProfiles { get; set; } = new List<PaymentProfile>();
        public ICollection<SuspensionLog> SuspensionLogs { get; set; } = new List<SuspensionLog>();

    }
}
