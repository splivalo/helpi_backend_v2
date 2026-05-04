using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpi.Infrastructure.Persistence.Configurations;

public class StudentServiceConfiguration : IEntityTypeConfiguration<StudentService>
{
    public void Configure(EntityTypeBuilder<StudentService> builder)
    {
        builder.HasKey(ss => new { ss.StudentId, ss.ServiceId });
    }
}
