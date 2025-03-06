using System.ComponentModel.DataAnnotations;

namespace Helpi.Domain.Entities
{

    public class StudentContract
    {
        public int Id { get; set; }
        public int StudentId { get; set; }

        [MaxLength(512)]
        public string CloudPath { get; set; } = null!;
        public DateOnly EffectiveDate { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Student Student { get; set; } = null!;
    }
}
