using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class CouponAssignmentConfiguration : IEntityTypeConfiguration<CouponAssignment>
{
    public void Configure(EntityTypeBuilder<CouponAssignment> builder)
    {
        builder.Property(a => a.RemainingValue).HasColumnType("decimal(18,2)");

        builder.HasOne(a => a.Coupon)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Senior)
            .WithMany()
            .HasForeignKey(a => a.SeniorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedByAdmin)
            .WithMany()
            .HasForeignKey(a => a.AssignedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
