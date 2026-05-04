using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(50);
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Value).HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.City)
            .WithMany()
            .HasForeignKey(c => c.CityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
