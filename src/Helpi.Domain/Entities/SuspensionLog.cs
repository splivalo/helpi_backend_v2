using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class SuspensionLog
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public SuspensionAction Action { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int AdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(AdminId))]
        public User AdminUser { get; set; } = null!;
    }
}
