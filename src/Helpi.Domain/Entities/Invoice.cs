using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class Invoice
    {
        public int Id { get; set; }
        public int JobInstanceId { get; set; }
        public int TransactionId { get; set; }

        [MaxLength(255)]
        public string? MailerliteCampaignId { get; set; }

        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = null!;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public DateOnly DueDate { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ViewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public JobInstance JobInstance { get; set; } = null!;
        public PaymentTransaction Transaction { get; set; } = null!;
        public InvoiceEmail? Email { get; set; }
    }
}
