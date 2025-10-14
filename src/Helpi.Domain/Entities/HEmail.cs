using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class HEmail
    {
        public int Id { get; set; }
        public int ExternalInvoiceId { get; set; } // Eg minimax

        public EmailType Type { get; set; } = EmailType.Invoice;
        public EmailStatus Status { get; set; } = EmailStatus.Queued;
        public int OpenedCount { get; set; } = 0;
        public DateTime? LastAttempt { get; set; }
        public DateTime? NextAttempt { get; set; }
        public int AttemptCount { get; set; } = 0;

        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }

        // public Invoice Invoice { get; set; } = null!;
    }
}
