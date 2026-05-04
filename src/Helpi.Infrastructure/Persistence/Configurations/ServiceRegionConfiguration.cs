using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class ServiceRegionConfiguration : IEntityTypeConfiguration<ServiceRegion>
{
    public void Configure(EntityTypeBuilder<ServiceRegion> builder)
    {
        builder.HasIndex(sr => new { sr.CityId, sr.ServiceId }).IsUnique();
    }
}
