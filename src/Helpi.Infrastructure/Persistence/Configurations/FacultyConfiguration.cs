using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Helpi.Infrastructure.Persistence.Configurations;
public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.HasData(

                    new Faculty { Id = 1, FacultyName = "Business and Economics" },
                    new Faculty { Id = 2, FacultyName = "Education" },
                    new Faculty { Id = 3, FacultyName = "Engineering" },
                    new Faculty { Id = 4, FacultyName = "Health Sciences" },
                    new Faculty { Id = 5, FacultyName = "Law" },
                    new Faculty { Id = 6, FacultyName = "Medicine" },
                    new Faculty { Id = 7, FacultyName = "Science" },
                    new Faculty { Id = 8, FacultyName = "Social Sciences" },
                    new Faculty { Id = 9, FacultyName = "Computer Science" },
                    new Faculty { Id = 10, FacultyName = "Arts and Humanities" }
        );
    }
}