using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class PricingConfigurationEntityConfiguration : IEntityTypeConfiguration<PricingConfiguration>
{
    public void Configure(EntityTypeBuilder<PricingConfiguration> builder)
    {
        builder.Property(p => p.JobHourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.SundayHourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.StudentHourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.StudentSundayHourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(p => p.CompanyPercentage).HasColumnType("decimal(5,2)");
        builder.Property(p => p.ServiceProviderPercentage).HasColumnType("decimal(5,2)");
        builder.Property(p => p.VatPercentage).HasColumnType("decimal(5,2)");
        builder.Property(p => p.IntermediaryPercentage).HasColumnType("decimal(5,2)");
    }
}
