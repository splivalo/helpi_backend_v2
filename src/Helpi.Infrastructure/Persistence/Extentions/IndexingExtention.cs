using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Persistence.Extentions
{
    public static class IndexingExtensions
    {
        public static void AddCustomIndexes(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderSchedule>()
                .HasIndex(x => x.Id)
                .HasDatabaseName("IX_OrderSchedules_Id");

            // Composite index on OrderSchedules timing (for conflict checks)
            modelBuilder.Entity<OrderSchedule>()
                .HasIndex(x => new { x.DayOfWeek, x.StartTime, x.EndTime })
                .HasDatabaseName("IX_OrderSchedules_DayOfWeek_Start_End");

            modelBuilder.Entity<Student>()
                .HasIndex(x => x.UserId)
                .HasDatabaseName("IX_Students_UserId");

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_Students_Status");

            modelBuilder.Entity<StudentAvailabilitySlot>()
                .HasIndex(x => new { x.StudentId, x.DayOfWeek, x.StartTime, x.EndTime })
                .HasDatabaseName("IX_AvailabilitySlots_Student_Day_Time");

            modelBuilder.Entity<StudentService>()
                .HasIndex(x => new { x.StudentId, x.ServiceId })
                .HasDatabaseName("IX_StudentServices_Student_Service");

            modelBuilder.Entity<ContactInfo>()
                .HasIndex(x => x.CityId)
                .HasDatabaseName("IX_Contacts_CityId");

            modelBuilder.Entity<ScheduleAssignment>()
                .HasIndex(x => new { x.StudentId, x.OrderScheduleId })
                .HasDatabaseName("IX_ScheduleAssignments_Student_OrderSchedule");

            modelBuilder.Entity<JobInstance>()
                .HasIndex(x => new { x.OrderId, x.Status })
                .IncludeProperties(x => x.ScheduledDate)
                .HasDatabaseName("IX_JobInstances_OrderStatus");

        }
    }
}
