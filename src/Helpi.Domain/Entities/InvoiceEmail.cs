using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class InvoiceEmail
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }

        [MaxLength(255)]
        public string? MailerliteMessageId { get; set; }
        public EmailStatus Status { get; set; } = EmailStatus.Queued;
        public int OpenedCount { get; set; } = 0;
        public DateTime? LastAttempt { get; set; }
        public int AttemptCount { get; set; } = 0;

        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }

        public Invoice Invoice { get; set; } = null!;
    }
}
