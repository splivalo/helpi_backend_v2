using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class StudentAvailabilitySlotConfiguration : IEntityTypeConfiguration<StudentAvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<StudentAvailabilitySlot> builder)
    {
        builder.HasKey(s => new { s.StudentId, s.DayOfWeek });

        builder.HasOne(s => s.Student)
            .WithMany(st => st.AvailabilitySlots)
            .HasForeignKey(s => s.StudentId);
    }
}
