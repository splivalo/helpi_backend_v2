using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class SeniorConfiguration : IEntityTypeConfiguration<Senior>
{
    public void Configure(EntityTypeBuilder<Senior> builder)
    {
        builder.Property(s => s.SpecialRequirements)
            .HasColumnType("jsonb");
    }
}
