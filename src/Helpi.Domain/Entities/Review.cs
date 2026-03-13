using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class Review
    {
        public int Id { get; set; }
        public ReviewType Type { get; set; } = ReviewType.SeniorToStudent;
        public int SeniorId { get; set; }
        public string SeniorFullName { get; set; } = null!;
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = null!;
        public int JobInstanceId { get; set; }
        public double Rating { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        public int RetryCount { get; set; } = 0;
        public int MaxRetry { get; set; } = 2;
        public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;
        public bool IsPending { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Senior Senior { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public JobInstance JobInstance { get; set; } = null!;
    }
}
