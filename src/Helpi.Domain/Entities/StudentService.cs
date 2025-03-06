namespace Helpi.Domain.Entities
{

    public class StudentService
    {
        public int StudentId { get; set; }
        public int ServiceId { get; set; }
        public byte? ExperienceYears { get; set; }

        public Student Student { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}
