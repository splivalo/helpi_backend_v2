using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.Property(u => u.CoveredAmount).HasColumnType("decimal(18,2)");
        builder.Property(u => u.CoveredHours).HasColumnType("decimal(18,2)");

        builder.HasOne(u => u.CouponAssignment)
            .WithMany(a => a.Usages)
            .HasForeignKey(u => u.CouponAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.JobInstance)
            .WithMany()
            .HasForeignKey(u => u.JobInstanceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
