using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class SuspensionLogConfiguration : IEntityTypeConfiguration<SuspensionLog>
{
    public void Configure(EntityTypeBuilder<SuspensionLog> builder)
    {
        builder.HasOne(s => s.User)
            .WithMany(u => u.SuspensionLogs)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AdminUser)
            .WithMany()
            .HasForeignKey(s => s.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
