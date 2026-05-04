using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
{
    public void Configure(EntityTypeBuilder<OrderService> builder)
    {
        builder.HasKey(os => new { os.OrderId, os.ServiceId });
    }
}
