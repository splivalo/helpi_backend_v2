using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class FcmTokenConfiguration : IEntityTypeConfiguration<FcmToken>
{
    public void Configure(EntityTypeBuilder<FcmToken> builder)
    {
        builder.HasKey(t => new { t.UserId, t.Token });
    }
}
