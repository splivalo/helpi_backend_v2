using System.ComponentModel.DataAnnotations.Schema;

namespace Helpi.Domain.Entities
{

    public class Review
    {
        public int Id { get; set; }
        public int SeniorId { get; set; }
        public int StudentId { get; set; }
        public int JobInstanceId { get; set; }
        public byte Rating { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Senior Senior { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public JobInstance JobInstance { get; set; } = null!;
    }
}
