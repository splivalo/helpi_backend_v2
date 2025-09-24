using System.ComponentModel.DataAnnotations.Schema;

namespace Helpi.Domain.Entities
{

    /// <summary>
    /// Primary key =DayOfWeek + StudentId 
    /// </summary>
    public class StudentAvailabilitySlot
    {

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public Student Student { get; set; } = null!;
    }
}
