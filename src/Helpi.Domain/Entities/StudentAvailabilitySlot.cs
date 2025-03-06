namespace Helpi.Domain.Entities
{

    public class StudentAvailabilitySlot
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public Student Student { get; set; } = null!;
    }
}
