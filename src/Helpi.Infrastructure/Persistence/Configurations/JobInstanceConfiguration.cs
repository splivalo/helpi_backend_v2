using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class JobInstanceConfiguration : IEntityTypeConfiguration<JobInstance>
{
    public void Configure(EntityTypeBuilder<JobInstance> builder)
    {
        builder.HasOne(j => j.ScheduleAssignment)
            .WithMany(s => s.JobInstances)
            .HasForeignKey(j => j.ScheduleAssignmentId);

        builder.Property(p => p.HourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.StudentHourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.CompanyPercentage).HasColumnType("decimal(5,2)");
        builder.Property(p => p.ServiceProviderPercentage).HasColumnType("decimal(5,2)");

        builder.Property(j => j.Notes)
            .HasMaxLength(1000)
            .IsUnicode(true);

        builder.HasOne(j => j.PaymentTransaction)
            .WithOne(p => p.JobInstance)
            .HasForeignKey<PaymentTransaction>(p => p.JobInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
