using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{
    public class Student
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(20)]
        public string? StudentNumber { get; set; }
        public int FacultyId { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
        public int ContactId { get; set; }
        public StudentStatus Status { get; set; } = StudentStatus.InActive;

        public int? DaysToContractExpire { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateOnly? BackgroundCheckDate { get; set; }

        public int TotalReviews { get; set; } = 0;
        public decimal TotalRatingSum { get; set; } = 0.00m;
        public decimal AverageRating { get; set; } = 0.00m;

        // public User User { get; set; } = null!;
        public ContactInfo Contact { get; set; } = null!;
        public Faculty Faculty { get; set; } = null!;
        public ICollection<StudentService> StudentServices { get; set; } = new List<StudentService>();
        public ICollection<StudentAvailabilitySlot> AvailabilitySlots { get; set; } = new List<StudentAvailabilitySlot>();

        public ICollection<ScheduleAssignment> ScheduleAssignments { get; set; } = new List<ScheduleAssignment>();
        public ICollection<StudentContract> Contracts { get; set; } = new List<StudentContract>();

        public StudentContract? ActiveContract
                => Contracts
                .Where(c => c.DeletedOn == null)
            .Where(c => c.Status == ContractStatus.Active)
            .OrderByDescending(c => c.ExpirationDate)
            .FirstOrDefault();


    }
}
