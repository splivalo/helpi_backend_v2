using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class SponsorConfiguration : IEntityTypeConfiguration<Sponsor>
{
    public void Configure(EntityTypeBuilder<Sponsor> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(200);
        builder.Property(s => s.LogoUrl).HasMaxLength(2000);
        builder.Property(s => s.DarkLogoUrl).HasMaxLength(2000);
        builder.Property(s => s.LinkUrl).HasMaxLength(2000);
        builder.Property(s => s.Label).HasColumnType("jsonb");
    }
}
