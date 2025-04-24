using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class StudentContract
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.valid;

        public string ContractNumber { get; set; } = null!;

        [MaxLength(512)]
        public string CloudPath { get; set; } = null!;
        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Student Student { get; set; } = null!;
    }
}
